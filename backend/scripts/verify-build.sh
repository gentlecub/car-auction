#!/bin/bash

# ============================================
# Car Auction Backend - Build Verification Script
# ============================================
# This script verifies that the solution builds correctly
# and all tests pass before deployment.
# ============================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}  Car Auction Backend - Build Verification  ${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

# Track results
ERRORS=0

# Function to print step
print_step() {
    echo -e "\n${YELLOW}▶ $1${NC}"
}

# Function to print success
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# Function to print error
print_error() {
    echo -e "${RED}✗ $1${NC}"
    ((ERRORS++))
}

# Navigate to solution directory
cd "$ROOT_DIR"

# ============================================
# Step 1: Clean previous builds
# ============================================
print_step "Cleaning previous builds..."
if dotnet clean --verbosity quiet 2>/dev/null; then
    print_success "Clean completed"
else
    print_error "Clean failed"
fi

# ============================================
# Step 2: Restore NuGet packages
# ============================================
print_step "Restoring NuGet packages..."
if dotnet restore --verbosity quiet; then
    print_success "Packages restored successfully"
else
    print_error "Package restore failed"
    exit 1
fi

# ============================================
# Step 3: Build Solution (Release mode)
# ============================================
print_step "Building solution (Release mode)..."
if dotnet build --configuration Release --no-restore --verbosity minimal; then
    print_success "Build completed successfully"
else
    print_error "Build failed"
    exit 1
fi

# ============================================
# Step 4: Run Unit Tests
# ============================================
print_step "Running unit tests..."
if dotnet test tests/CarAuction.UnitTests/CarAuction.UnitTests.csproj \
    --configuration Release \
    --no-build \
    --verbosity normal \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage"; then
    print_success "Unit tests passed"
else
    print_error "Unit tests failed"
fi

# ============================================
# Step 5: Run Integration Tests
# ============================================
print_step "Running integration tests..."
if dotnet test tests/CarAuction.IntegrationTests/CarAuction.IntegrationTests.csproj \
    --configuration Release \
    --no-build \
    --verbosity normal \
    --logger "console;verbosity=detailed"; then
    print_success "Integration tests passed"
else
    print_error "Integration tests failed"
fi

# ============================================
# Step 6: Check for Security Vulnerabilities
# ============================================
print_step "Checking for security vulnerabilities..."
if dotnet list package --vulnerable --include-transitive 2>/dev/null | grep -q "has no vulnerable packages"; then
    print_success "No vulnerable packages found"
elif dotnet list package --vulnerable --include-transitive 2>/dev/null | grep -q "vulnerable"; then
    print_error "Vulnerable packages detected - review output above"
else
    print_success "Security check completed"
fi

# ============================================
# Step 7: Verify Docker Build
# ============================================
print_step "Verifying Docker build..."
if command -v docker &> /dev/null; then
    if docker build -t carauction-api:verify -f src/CarAuction.API/Dockerfile . --quiet 2>/dev/null; then
        print_success "Docker image built successfully"
        # Clean up verification image
        docker rmi carauction-api:verify --force 2>/dev/null || true
    else
        print_error "Docker build failed"
    fi
else
    echo -e "${YELLOW}  ⚠ Docker not installed - skipping Docker build verification${NC}"
fi

# ============================================
# Step 8: Publish Application
# ============================================
print_step "Publishing application..."
PUBLISH_DIR="$ROOT_DIR/publish"
rm -rf "$PUBLISH_DIR" 2>/dev/null || true

if dotnet publish src/CarAuction.API/CarAuction.API.csproj \
    --configuration Release \
    --output "$PUBLISH_DIR" \
    --no-build \
    --verbosity minimal; then
    print_success "Application published to: $PUBLISH_DIR"

    # Show published files count
    FILE_COUNT=$(find "$PUBLISH_DIR" -type f | wc -l)
    echo -e "  📦 Published files: $FILE_COUNT"
else
    print_error "Publish failed"
fi

# ============================================
# Summary
# ============================================
echo ""
echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}             VERIFICATION SUMMARY           ${NC}"
echo -e "${BLUE}============================================${NC}"

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}"
    echo "  ✓ All verifications passed!"
    echo "  ✓ Solution is ready for deployment"
    echo -e "${NC}"
    exit 0
else
    echo -e "${RED}"
    echo "  ✗ $ERRORS verification(s) failed"
    echo "  ✗ Please fix issues before deployment"
    echo -e "${NC}"
    exit 1
fi
