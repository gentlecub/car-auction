# ============================================
# Car Auction Backend - Build Verification Script
# PowerShell Version for Windows
# ============================================

$ErrorActionPreference = "Stop"

# Colors
function Write-Step { param($Message) Write-Host "`n▶ $Message" -ForegroundColor Yellow }
function Write-Success { param($Message) Write-Host "✓ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "✗ $Message" -ForegroundColor Red; $script:Errors++ }
function Write-Warning { param($Message) Write-Host "⚠ $Message" -ForegroundColor DarkYellow }

$script:Errors = 0
$RootDir = Split-Path -Parent $PSScriptRoot

Write-Host "============================================" -ForegroundColor Blue
Write-Host "  Car Auction Backend - Build Verification  " -ForegroundColor Blue
Write-Host "============================================" -ForegroundColor Blue

Set-Location $RootDir

# Step 1: Clean
Write-Step "Cleaning previous builds..."
try {
    dotnet clean --verbosity quiet 2>$null
    Write-Success "Clean completed"
} catch {
    Write-Error "Clean failed"
}

# Step 2: Restore
Write-Step "Restoring NuGet packages..."
try {
    dotnet restore --verbosity quiet
    Write-Success "Packages restored successfully"
} catch {
    Write-Error "Package restore failed"
    exit 1
}

# Step 3: Build
Write-Step "Building solution (Release mode)..."
try {
    dotnet build --configuration Release --no-restore --verbosity minimal
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build completed successfully"
    } else {
        throw "Build failed"
    }
} catch {
    Write-Error "Build failed: $_"
    exit 1
}

# Step 4: Unit Tests
Write-Step "Running unit tests..."
try {
    dotnet test tests/CarAuction.UnitTests/CarAuction.UnitTests.csproj `
        --configuration Release `
        --no-build `
        --verbosity normal `
        --collect:"XPlat Code Coverage"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Unit tests passed"
    } else {
        throw "Tests failed"
    }
} catch {
    Write-Error "Unit tests failed: $_"
}

# Step 5: Integration Tests
Write-Step "Running integration tests..."
try {
    dotnet test tests/CarAuction.IntegrationTests/CarAuction.IntegrationTests.csproj `
        --configuration Release `
        --no-build `
        --verbosity normal
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Integration tests passed"
    } else {
        throw "Tests failed"
    }
} catch {
    Write-Error "Integration tests failed: $_"
}

# Step 6: Security Check
Write-Step "Checking for security vulnerabilities..."
try {
    $output = dotnet list package --vulnerable --include-transitive 2>&1
    if ($output -match "no vulnerable packages") {
        Write-Success "No vulnerable packages found"
    } elseif ($output -match "vulnerable") {
        Write-Error "Vulnerable packages detected"
        Write-Host $output
    } else {
        Write-Success "Security check completed"
    }
} catch {
    Write-Warning "Could not check for vulnerabilities"
}

# Step 7: Docker Build (if available)
Write-Step "Verifying Docker build..."
$dockerExists = Get-Command docker -ErrorAction SilentlyContinue
if ($dockerExists) {
    try {
        docker build -t carauction-api:verify -f src/CarAuction.API/Dockerfile . --quiet 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Docker image built successfully"
            docker rmi carauction-api:verify --force 2>$null | Out-Null
        } else {
            throw "Docker build failed"
        }
    } catch {
        Write-Error "Docker build failed: $_"
    }
} else {
    Write-Warning "Docker not installed - skipping Docker build verification"
}

# Step 8: Publish
Write-Step "Publishing application..."
$PublishDir = Join-Path $RootDir "publish"
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}

try {
    dotnet publish src/CarAuction.API/CarAuction.API.csproj `
        --configuration Release `
        --output $PublishDir `
        --no-build `
        --verbosity minimal
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Application published to: $PublishDir"
        $fileCount = (Get-ChildItem -Recurse -File $PublishDir).Count
        Write-Host "  📦 Published files: $fileCount" -ForegroundColor Cyan
    } else {
        throw "Publish failed"
    }
} catch {
    Write-Error "Publish failed: $_"
}

# Summary
Write-Host "`n============================================" -ForegroundColor Blue
Write-Host "             VERIFICATION SUMMARY           " -ForegroundColor Blue
Write-Host "============================================" -ForegroundColor Blue

if ($script:Errors -eq 0) {
    Write-Host "`n  ✓ All verifications passed!" -ForegroundColor Green
    Write-Host "  ✓ Solution is ready for deployment`n" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n  ✗ $($script:Errors) verification(s) failed" -ForegroundColor Red
    Write-Host "  ✗ Please fix issues before deployment`n" -ForegroundColor Red
    exit 1
}
