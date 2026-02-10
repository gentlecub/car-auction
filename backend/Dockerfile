# ============================================
# CarAuction Backend - Railway Dockerfile
# Optimized for Railway.app deployment with Swagger enabled
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for dependency restoration
COPY ["src/CarAuction.Domain/CarAuction.Domain.csproj", "CarAuction.Domain/"]
COPY ["src/CarAuction.Application/CarAuction.Application.csproj", "CarAuction.Application/"]
COPY ["src/CarAuction.Infrastructure/CarAuction.Infrastructure.csproj", "CarAuction.Infrastructure/"]
COPY ["src/CarAuction.API/CarAuction.API.csproj", "CarAuction.API/"]

# Restore dependencies
RUN dotnet restore "CarAuction.API/CarAuction.API.csproj"

# Copy all source code
COPY src/ .

# Build in Release mode
WORKDIR /src/CarAuction.API
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --uid 1000 appuser

# Copy published files
COPY --from=publish /app/publish .

# Create uploads directory and set permissions
RUN mkdir -p /app/wwwroot/uploads && \
    mkdir -p /app/logs && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Railway assigns PORT dynamically - we use 5000 as default
ENV PORT=5000
ENV ASPNETCORE_URLS=http://+:${PORT}

# Enable Swagger in demo/staging (Railway deployment)
# This is controlled by ENABLE_SWAGGER_IN_PRODUCTION env var
ENV ENABLE_SWAGGER_IN_PRODUCTION=true

# Health check using the dynamic port
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:${PORT}/health || exit 1

# Entry point with shell to expand PORT variable
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet CarAuction.API.dll"]
