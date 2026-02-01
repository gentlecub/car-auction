# Docker & DevOps

Guía completa de containerización, CI/CD y despliegue del backend CarAuction.

---

## Estado Actual

| Componente | Estado |
|------------|--------|
| Dockerfile | No existe |
| docker-compose.yml | No existe |
| .env | No existe |
| CI/CD Pipeline | No configurado |

---

## Arquitectura de Despliegue

```
┌─────────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA PRODUCCIÓN                      │
└─────────────────────────────────────────────────────────────────┘

                         ┌─────────────┐
                         │   Nginx     │
                         │   Reverse   │
                         │   Proxy     │
                         │   :80/:443  │
                         └──────┬──────┘
                                │
                 ┌──────────────┼──────────────┐
                 │              │              │
                 ▼              ▼              ▼
          ┌───────────┐  ┌───────────┐  ┌───────────┐
          │  API #1   │  │  API #2   │  │  API #3   │
          │  :5000    │  │  :5000    │  │  :5000    │
          └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
                │              │              │
                └──────────────┼──────────────┘
                               │
                 ┌─────────────┼─────────────┐
                 │             │             │
                 ▼             ▼             ▼
          ┌───────────┐ ┌───────────┐ ┌───────────┐
          │   MySQL   │ │   Redis   │ │  SignalR  │
          │   :3306   │ │   :6379   │ │  Backplane│
          └───────────┘ └───────────┘ └───────────┘
```

---

## Archivos Docker

### Dockerfile

Crear en `/backend/Dockerfile`:

```dockerfile
# ============================================
# STAGE 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias
COPY ["src/CarAuction.Domain/CarAuction.Domain.csproj", "CarAuction.Domain/"]
COPY ["src/CarAuction.Application/CarAuction.Application.csproj", "CarAuction.Application/"]
COPY ["src/CarAuction.Infrastructure/CarAuction.Infrastructure.csproj", "CarAuction.Infrastructure/"]
COPY ["src/CarAuction.API/CarAuction.API.csproj", "CarAuction.API/"]

# Restaurar dependencias
RUN dotnet restore "CarAuction.API/CarAuction.API.csproj"

# Copiar todo el código fuente
COPY src/ .

# Build en modo Release
WORKDIR /src/CarAuction.API
RUN dotnet build -c Release -o /app/build

# ============================================
# STAGE 2: Publish
# ============================================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# STAGE 3: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' appuser

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Cambiar ownership
RUN chown -R appuser:appuser /app

# Usar usuario no-root
USER appuser

# Exponer puerto
EXPOSE 5000

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Entrypoint
ENTRYPOINT ["dotnet", "CarAuction.API.dll"]
```

---

### Dockerfile.dev (Desarrollo)

Crear en `/backend/Dockerfile.dev`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Instalar herramientas de desarrollo
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Exponer puertos
EXPOSE 5000
EXPOSE 5001

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=1

# Comando por defecto (hot reload)
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/CarAuction.API/CarAuction.API.csproj"]
```

---

### docker-compose.yml

Crear en `/backend/docker-compose.yml`:

```yaml
version: '3.8'

services:
  # ============================================
  # API Backend
  # ============================================
  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: carauction-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=carauction;User=carauction;Password=${MYSQL_PASSWORD};CharSet=utf8mb4;
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - JwtSettings__Issuer=CarAuction
      - JwtSettings__Audience=CarAuctionClient
      - JwtSettings__AccessTokenExpirationMinutes=60
      - JwtSettings__RefreshTokenExpirationDays=7
      - Redis__ConnectionString=redis:6379
    depends_on:
      mysql:
        condition: service_healthy
      redis:
        condition: service_started
    networks:
      - carauction-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  # ============================================
  # MySQL Database
  # ============================================
  mysql:
    image: mysql:8.0
    container_name: carauction-mysql
    ports:
      - "3306:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD}
      - MYSQL_DATABASE=carauction
      - MYSQL_USER=carauction
      - MYSQL_PASSWORD=${MYSQL_PASSWORD}
    volumes:
      - mysql-data:/var/lib/mysql
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql:ro
    networks:
      - carauction-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-p${MYSQL_ROOT_PASSWORD}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
    command: --default-authentication-plugin=mysql_native_password --character-set-server=utf8mb4 --collation-server=utf8mb4_unicode_ci

  # ============================================
  # Redis (Cache & SignalR Backplane)
  # ============================================
  redis:
    image: redis:7-alpine
    container_name: carauction-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - carauction-network
    restart: unless-stopped
    command: redis-server --appendonly yes

  # ============================================
  # Nginx Reverse Proxy (Opcional)
  # ============================================
  nginx:
    image: nginx:alpine
    container_name: carauction-nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    depends_on:
      - api
    networks:
      - carauction-network
    restart: unless-stopped
    profiles:
      - production

# ============================================
# Volumes
# ============================================
volumes:
  mysql-data:
    driver: local
  redis-data:
    driver: local

# ============================================
# Networks
# ============================================
networks:
  carauction-network:
    driver: bridge
```

---

### docker-compose.dev.yml

Crear en `/backend/docker-compose.dev.yml`:

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.dev
    container_name: carauction-api-dev
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=carauction_dev;User=root;Password=devpassword;CharSet=utf8mb4;
      - JwtSettings__SecretKey=dev-secret-key-minimum-32-characters-long
      - JwtSettings__Issuer=CarAuction
      - JwtSettings__Audience=CarAuctionClient
      - JwtSettings__AccessTokenExpirationMinutes=60
      - JwtSettings__RefreshTokenExpirationDays=7
    volumes:
      - .:/app
      - /app/src/CarAuction.API/bin
      - /app/src/CarAuction.API/obj
    depends_on:
      - mysql
    networks:
      - carauction-dev

  mysql:
    image: mysql:8.0
    container_name: carauction-mysql-dev
    ports:
      - "3306:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=devpassword
      - MYSQL_DATABASE=carauction_dev
    volumes:
      - mysql-dev-data:/var/lib/mysql
    networks:
      - carauction-dev
    command: --default-authentication-plugin=mysql_native_password

  adminer:
    image: adminer
    container_name: carauction-adminer
    ports:
      - "8080:8080"
    networks:
      - carauction-dev

volumes:
  mysql-dev-data:

networks:
  carauction-dev:
    driver: bridge
```

---

## Variables de Entorno

### .env.example

Crear en `/backend/.env.example`:

```bash
# ============================================
# DATABASE
# ============================================
MYSQL_ROOT_PASSWORD=your_secure_root_password
MYSQL_PASSWORD=your_secure_app_password

# ============================================
# JWT AUTHENTICATION
# ============================================
# Mínimo 32 caracteres, usar generador seguro
JWT_SECRET_KEY=your-super-secret-key-minimum-32-chars

# ============================================
# APPLICATION
# ============================================
ASPNETCORE_ENVIRONMENT=Production

# ============================================
# REDIS (opcional)
# ============================================
REDIS_PASSWORD=

# ============================================
# EMAIL (SMTP)
# ============================================
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USER=noreply@carauction.com
SMTP_PASSWORD=your_smtp_password
SMTP_FROM_EMAIL=noreply@carauction.com
SMTP_FROM_NAME=CarAuction

# ============================================
# CORS
# ============================================
CORS_ORIGINS=https://carauction.com,https://www.carauction.com
```

### .env (Desarrollo Local)

Crear en `/backend/.env`:

```bash
MYSQL_ROOT_PASSWORD=devpassword
MYSQL_PASSWORD=devpassword
JWT_SECRET_KEY=development-secret-key-minimum-32-characters
ASPNETCORE_ENVIRONMENT=Development
```

**IMPORTANTE**: Agregar `.env` a `.gitignore`

---

## Configuración Nginx

### nginx/nginx.conf

Crear en `/backend/nginx/nginx.conf`:

```nginx
events {
    worker_connections 1024;
}

http {
    # Upstream para la API (load balancing)
    upstream api_servers {
        least_conn;
        server api:5000;
        # Agregar más instancias para escalar:
        # server api2:5000;
        # server api3:5000;
    }

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
    limit_req_zone $binary_remote_addr zone=auth_limit:10m rate=5r/m;

    server {
        listen 80;
        server_name localhost;

        # Redirect HTTP to HTTPS (producción)
        # return 301 https://$server_name$request_uri;

        location / {
            proxy_pass http://api_servers;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_cache_bypass $http_upgrade;

            # Rate limiting general
            limit_req zone=api_limit burst=20 nodelay;
        }

        # SignalR WebSocket
        location /hubs {
            proxy_pass http://api_servers;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_read_timeout 86400;
        }

        # Rate limiting estricto para auth
        location /api/v1/auth {
            proxy_pass http://api_servers;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

            limit_req zone=auth_limit burst=5 nodelay;
        }

        # Health check endpoint
        location /health {
            proxy_pass http://api_servers;
            proxy_set_header Host $host;
        }
    }

    # HTTPS Server (producción)
    # server {
    #     listen 443 ssl http2;
    #     server_name carauction.com;
    #
    #     ssl_certificate /etc/nginx/ssl/cert.pem;
    #     ssl_certificate_key /etc/nginx/ssl/key.pem;
    #     ssl_protocols TLSv1.2 TLSv1.3;
    #
    #     # ... misma configuración de locations
    # }
}
```

---

## Scripts de Inicialización

### scripts/init-db.sql

Crear en `/backend/scripts/init-db.sql`:

```sql
-- Crear base de datos si no existe
CREATE DATABASE IF NOT EXISTS carauction
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE carauction;

-- Seed inicial de roles
INSERT INTO Roles (Id, Name, Description, CreatedAt)
VALUES
    (1, 'Admin', 'Administrador del sistema', UTC_TIMESTAMP()),
    (2, 'User', 'Usuario estándar', UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE Name = VALUES(Name);

-- Crear usuario admin por defecto
-- Password: Admin123! (hash BCrypt)
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, Status, EmailVerified, CreatedAt)
VALUES (
    1,
    'admin@carauction.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.4HqoQz4L4XK/Oi',
    'Admin',
    'System',
    1,
    1,
    UTC_TIMESTAMP()
)
ON DUPLICATE KEY UPDATE Email = VALUES(Email);

-- Asignar rol admin
INSERT INTO UserRoles (UserId, RoleId)
VALUES (1, 1)
ON DUPLICATE KEY UPDATE RoleId = VALUES(RoleId);
```

---

## Comandos Docker

### Desarrollo

```bash
# Iniciar entorno de desarrollo
docker-compose -f docker-compose.dev.yml up -d

# Ver logs en tiempo real
docker-compose -f docker-compose.dev.yml logs -f api

# Detener
docker-compose -f docker-compose.dev.yml down

# Reconstruir después de cambios en Dockerfile
docker-compose -f docker-compose.dev.yml up -d --build

# Ejecutar migraciones
docker-compose -f docker-compose.dev.yml exec api \
    dotnet ef database update -p src/CarAuction.Infrastructure -s src/CarAuction.API
```

### Producción

```bash
# Crear archivo .env desde ejemplo
cp .env.example .env
# Editar .env con valores de producción

# Construir imagen
docker-compose build

# Iniciar servicios
docker-compose up -d

# Iniciar con Nginx (perfil production)
docker-compose --profile production up -d

# Ver estado
docker-compose ps

# Ver logs
docker-compose logs -f

# Escalar API (múltiples instancias)
docker-compose up -d --scale api=3

# Detener todo
docker-compose down

# Detener y eliminar volúmenes (CUIDADO: borra datos)
docker-compose down -v
```

### Mantenimiento

```bash
# Backup de base de datos
docker-compose exec mysql mysqldump -u root -p carauction > backup_$(date +%Y%m%d).sql

# Restaurar backup
docker-compose exec -T mysql mysql -u root -p carauction < backup.sql

# Limpiar imágenes no usadas
docker image prune -a

# Ver uso de recursos
docker stats

# Acceder al contenedor
docker-compose exec api bash
docker-compose exec mysql mysql -u root -p
```

---

## CI/CD Pipeline

### GitHub Actions

Crear en `.github/workflows/ci-cd.yml`:

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}/carauction-api

jobs:
  # ============================================
  # BUILD & TEST
  # ============================================
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Restore dependencies
        run: dotnet restore
        working-directory: ./backend

      - name: Build
        run: dotnet build --no-restore -c Release
        working-directory: ./backend

      - name: Run Unit Tests
        run: dotnet test tests/CarAuction.UnitTests --no-build -c Release --verbosity normal
        working-directory: ./backend

      - name: Run Integration Tests
        run: dotnet test tests/CarAuction.IntegrationTests --no-build -c Release --verbosity normal
        working-directory: ./backend

  # ============================================
  # BUILD DOCKER IMAGE
  # ============================================
  build-docker:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push'

    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Login to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=sha,prefix=
            type=raw,value=latest,enable=${{ github.ref == 'refs/heads/main' }}

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: ./backend
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}

  # ============================================
  # DEPLOY TO STAGING
  # ============================================
  deploy-staging:
    needs: build-docker
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: staging

    steps:
      - name: Deploy to Staging
        uses: appleboy/ssh-action@v1.0.0
        with:
          host: ${{ secrets.STAGING_HOST }}
          username: ${{ secrets.STAGING_USER }}
          key: ${{ secrets.STAGING_SSH_KEY }}
          script: |
            cd /opt/carauction
            docker-compose pull
            docker-compose up -d
            docker image prune -f

  # ============================================
  # DEPLOY TO PRODUCTION
  # ============================================
  deploy-production:
    needs: build-docker
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production

    steps:
      - name: Deploy to Production
        uses: appleboy/ssh-action@v1.0.0
        with:
          host: ${{ secrets.PROD_HOST }}
          username: ${{ secrets.PROD_USER }}
          key: ${{ secrets.PROD_SSH_KEY }}
          script: |
            cd /opt/carauction
            docker-compose pull
            docker-compose up -d --no-deps api
            docker image prune -f
```

### Secrets Requeridos

Configurar en GitHub → Settings → Secrets:

| Secret | Descripción |
|--------|-------------|
| `STAGING_HOST` | IP/hostname del servidor staging |
| `STAGING_USER` | Usuario SSH para staging |
| `STAGING_SSH_KEY` | Clave privada SSH |
| `PROD_HOST` | IP/hostname del servidor producción |
| `PROD_USER` | Usuario SSH para producción |
| `PROD_SSH_KEY` | Clave privada SSH |

---

## Health Checks

### Endpoint de Health Check

Agregar en `Program.cs`:

```csharp
// Agregar health checks
builder.Services.AddHealthChecks()
    .AddMySql(connectionString, name: "mysql")
    .AddRedis(redisConnectionString, name: "redis");

// En el pipeline
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

### Respuesta Health Check

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "mysql", "status": "Healthy", "duration": 15.5 },
    { "name": "redis", "status": "Healthy", "duration": 2.3 }
  ]
}
```

---

## Checklist de Despliegue

### Pre-despliegue

- [ ] Variables de entorno configuradas
- [ ] JWT_SECRET_KEY seguro (mín. 32 chars)
- [ ] MYSQL_PASSWORD seguro
- [ ] Certificados SSL configurados
- [ ] Backups de base de datos
- [ ] Tests pasando

### Despliegue

- [ ] Pull de última imagen
- [ ] Migraciones ejecutadas
- [ ] Health check OK
- [ ] Logs sin errores
- [ ] SignalR conectando

### Post-despliegue

- [ ] Monitoreo activo
- [ ] Alertas configuradas
- [ ] Rollback plan documentado

---

## Estructura Final de Archivos

```
/backend
├── Dockerfile
├── Dockerfile.dev
├── docker-compose.yml
├── docker-compose.dev.yml
├── .env.example
├── .env                    # (gitignored)
├── .dockerignore
├── /nginx
│   └── nginx.conf
├── /scripts
│   └── init-db.sql
└── .github
    └── workflows
        └── ci-cd.yml
```

---

## .dockerignore

Crear en `/backend/.dockerignore`:

```
**/.git
**/.gitignore
**/.vs
**/.vscode
**/.idea
**/bin
**/obj
**/out
**/node_modules
**/.env
**/.env.*
!.env.example
**/docker-compose*.yml
**/Dockerfile*
**/*.md
**/tests
**/*.log
```

---

## Archivos a Crear (Resumen)

| Archivo | Prioridad | Descripción |
|---------|-----------|-------------|
| `Dockerfile` | Alta | Build multi-stage producción |
| `docker-compose.yml` | Alta | Orquestación de servicios |
| `.env.example` | Alta | Template de variables |
| `.dockerignore` | Alta | Exclusiones de build |
| `docker-compose.dev.yml` | Media | Entorno desarrollo |
| `Dockerfile.dev` | Media | Build desarrollo con hot reload |
| `nginx/nginx.conf` | Media | Reverse proxy y load balancing |
| `scripts/init-db.sql` | Media | Seed inicial de base de datos |
| `.github/workflows/ci-cd.yml` | Media | Pipeline CI/CD |
