# DOCKER-BACKEND.md — Dockerfile ASP.NET Core 8

## Estado Actual

El backend **ya cuenta con un Dockerfile optimizado** en `/backend/Dockerfile`:

| Característica | Estado |
|----------------|--------|
| Multi-stage build | ✅ Implementado |
| Usuario no-root | ✅ `appuser` UID 1000 |
| Health check | ✅ `/health` endpoint |
| Imagen base optimizada | ✅ `aspnet:8.0` |
| Restauración cacheada | ✅ `.csproj` primero |

---

## Análisis del Dockerfile Existente

```dockerfile
# Stage 1: Build (SDK completo ~700MB)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Restaura dependencias (cacheable)
# Compila en Release

# Stage 2: Publish
FROM build AS publish
# Genera artefactos optimizados

# Stage 3: Runtime (solo runtime ~200MB)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# Imagen final ~220MB con dependencias
```

---

## Optimizaciones Recomendadas

### 1. Usar Alpine para reducir tamaño

```dockerfile
# Cambiar de:
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# A:
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Resultado: ~110MB vs ~220MB
```

### 2. Habilitar AOT/Trimming (opcional)

```dockerfile
# En stage publish, agregar trimming
RUN dotnet publish -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishTrimmed=true \
    /p:TrimMode=link
```

### 3. Build con caché de NuGet

```dockerfile
# Antes de restore, agregar:
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "CarAuction.API/CarAuction.API.csproj"
```

---

## Dockerfile Backend Optimizado

```dockerfile
# backend/Dockerfile.optimized
# ============================================
# CarAuction Backend - Optimized Production
# ============================================

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Restaurar con caché
COPY ["src/CarAuction.Domain/CarAuction.Domain.csproj", "CarAuction.Domain/"]
COPY ["src/CarAuction.Application/CarAuction.Application.csproj", "CarAuction.Application/"]
COPY ["src/CarAuction.Infrastructure/CarAuction.Infrastructure.csproj", "CarAuction.Infrastructure/"]
COPY ["src/CarAuction.API/CarAuction.API.csproj", "CarAuction.API/"]

RUN dotnet restore "CarAuction.API/CarAuction.API.csproj"

COPY src/ .
WORKDIR /src/CarAuction.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Alpine
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Dependencias mínimas
RUN apk add --no-cache curl icu-libs

# Usuario no-root
RUN adduser -D -u 1000 appuser
COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "CarAuction.API.dll"]
```

---

## Comparativa de Tamaños

| Versión | Imagen Base | Tamaño Final |
|---------|-------------|--------------|
| Actual | `aspnet:8.0` | ~220MB |
| Optimizada | `aspnet:8.0-alpine` | ~110MB |
| Con Trimming | Alpine + Trim | ~80MB |

---

## Dockerfile Desarrollo

Ya existe en `/backend/Dockerfile.dev`:

```dockerfile
# Habilita hot reload con dotnet watch
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src
COPY . .
RUN dotnet restore
EXPOSE 5000
CMD ["dotnet", "watch", "run", "--project", "src/CarAuction.API"]
```

---

## Comandos de Build

```bash
# Build producción
docker build -t carauction-api:latest ./backend

# Build con versión específica
docker build -t carauction-api:1.0.0 \
  --build-arg BUILD_VERSION=1.0.0 \
  ./backend

# Ejecutar standalone
docker run -d -p 5000:5000 \
  -e "ConnectionStrings__DefaultConnection=..." \
  -e "JwtSettings__SecretKey=..." \
  carauction-api:latest
```

---

**🛑 CONTINÚA leyendo para Módulo 2.3: Docker Compose Unificado**
