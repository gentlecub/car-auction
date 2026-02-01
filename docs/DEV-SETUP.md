# DEV-SETUP.md — Guía de Desarrollo Local

## Requisitos Previos

| Herramienta | Versión Mínima | Verificar |
|-------------|----------------|-----------|
| Docker | 20.10+ | `docker --version` |
| Docker Compose | 2.0+ | `docker-compose --version` |
| Node.js | 20+ | `node --version` |
| .NET SDK | 8.0 | `dotnet --version` |
| Git | 2.0+ | `git --version` |

---

## Opción 1: Docker Compose (Recomendado)

### Ejecutar Full Stack con un comando

```bash
# Desde la raíz del proyecto
cd "/mnt/d/project/car auction"

# Iniciar todos los servicios
docker-compose -f docker-compose.dev.yml up -d

# Ver logs en tiempo real
docker-compose -f docker-compose.dev.yml logs -f
```

### URLs de Desarrollo

| Servicio | URL | Descripción |
|----------|-----|-------------|
| Frontend | http://localhost:5173 | Vite dev server (hot reload) |
| API | http://localhost:5000 | ASP.NET Core API |
| Swagger | http://localhost:5000/swagger | Documentación API |
| MySQL | localhost:3306 | Base de datos |
| Redis | localhost:6379 | Cache |

### Comandos Útiles

```bash
# Ver estado de contenedores
docker-compose -f docker-compose.dev.yml ps

# Reiniciar un servicio específico
docker-compose -f docker-compose.dev.yml restart frontend

# Ver logs de un servicio
docker-compose -f docker-compose.dev.yml logs -f api

# Detener todo
docker-compose -f docker-compose.dev.yml down

# Detener y eliminar volúmenes (reset DB)
docker-compose -f docker-compose.dev.yml down -v
```

---

## Opción 2: Ejecución Manual (Sin Docker)

### 1. Base de Datos (MySQL)

```bash
# Opción A: Usar Docker solo para MySQL
docker run -d \
  --name carauction-mysql \
  -p 3306:3306 \
  -e MYSQL_ROOT_PASSWORD=rootpass123 \
  -e MYSQL_DATABASE=carauction \
  -e MYSQL_USER=carauction \
  -e MYSQL_PASSWORD=devpass123 \
  mysql:8.0

# Opción B: Instalar MySQL localmente y crear DB
mysql -u root -p
CREATE DATABASE carauction;
CREATE USER 'carauction'@'localhost' IDENTIFIED BY 'devpass123';
GRANT ALL PRIVILEGES ON carauction.* TO 'carauction'@'localhost';
```

### 2. Redis (Opcional)

```bash
# Usar Docker
docker run -d --name carauction-redis -p 6379:6379 redis:7-alpine

# O instalar localmente
# Windows: https://github.com/microsoftarchive/redis/releases
# Linux: sudo apt install redis-server
```

### 3. Backend (.NET)

```bash
# Navegar al backend
cd backend

# Restaurar dependencias
dotnet restore

# Configurar variables de entorno (PowerShell)
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=carauction;User=carauction;Password=devpass123;"
$env:JwtSettings__SecretKey="dev-secret-key-minimum-32-characters-here"

# Configurar variables de entorno (Bash)
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=carauction;User=carauction;Password=devpass123;"
export JwtSettings__SecretKey="dev-secret-key-minimum-32-characters-here"

# Ejecutar migraciones
dotnet ef database update --project src/CarAuction.Infrastructure

# Iniciar API
dotnet run --project src/CarAuction.API

# API disponible en: http://localhost:5000
```

### 4. Frontend (React)

```bash
# Navegar al frontend
cd frontend

# Instalar dependencias
npm install

# Iniciar servidor de desarrollo
npm run dev

# Frontend disponible en: http://localhost:5173
```

---

## Variables de Entorno

### Frontend (.env.development)

```env
VITE_API_URL=http://localhost:5000/api
VITE_WS_URL=ws://localhost:5000/hubs/auction
VITE_ENV=development
```

### Backend (appsettings.Development.json o variables)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=carauction;User=carauction;Password=devpass123;"
  },
  "JwtSettings": {
    "SecretKey": "dev-secret-key-minimum-32-characters-here",
    "Issuer": "CarAuction",
    "Audience": "CarAuctionClient"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

---

## Troubleshooting

### Puerto en uso

```bash
# Ver qué usa el puerto (Windows)
netstat -ano | findstr :5000

# Ver qué usa el puerto (Linux/Mac)
lsof -i :5000

# Matar proceso
kill -9 <PID>
```

### Docker no inicia

```bash
# Reiniciar Docker Desktop (Windows/Mac)
# O en Linux:
sudo systemctl restart docker
```

### Error de conexión a MySQL

```bash
# Verificar que MySQL está corriendo
docker ps | grep mysql

# Ver logs de MySQL
docker logs carauction-mysql

# Conectar manualmente para probar
mysql -h localhost -P 3306 -u carauction -pdevpass123 carauction
```

### Frontend no conecta con API

1. Verificar que API está corriendo: `curl http://localhost:5000/health`
2. Verificar CORS en backend permite `http://localhost:5173`
3. Verificar `.env.development` tiene URL correcta

### Limpiar y reiniciar todo

```bash
# Detener contenedores
docker-compose -f docker-compose.dev.yml down -v

# Eliminar imágenes locales
docker rmi carauction-frontend-dev carauction-api-dev

# Reconstruir
docker-compose -f docker-compose.dev.yml up -d --build
```

---

## Scripts de Desarrollo

### Crear archivo de inicio rápido

```bash
# /scripts/dev-start.sh
#!/bin/bash
echo "🚀 Starting CarAuction Development Environment..."
docker-compose -f docker-compose.dev.yml up -d
echo ""
echo "✅ Services started!"
echo "   Frontend: http://localhost:5173"
echo "   API:      http://localhost:5000"
echo "   Swagger:  http://localhost:5000/swagger"
echo ""
echo "📋 Logs: docker-compose -f docker-compose.dev.yml logs -f"
```

```powershell
# /scripts/dev-start.ps1
Write-Host "🚀 Starting CarAuction Development Environment..." -ForegroundColor Cyan
docker-compose -f docker-compose.dev.yml up -d
Write-Host ""
Write-Host "✅ Services started!" -ForegroundColor Green
Write-Host "   Frontend: http://localhost:5173"
Write-Host "   API:      http://localhost:5000"
Write-Host "   Swagger:  http://localhost:5000/swagger"
```

---

## Flujo de Trabajo Recomendado

```
1. Iniciar servicios
   └── docker-compose -f docker-compose.dev.yml up -d

2. Desarrollar con hot reload
   ├── Frontend: Cambios en /src → Auto-refresh
   └── Backend: Cambios en /src → Requiere rebuild

3. Ver logs si hay errores
   └── docker-compose -f docker-compose.dev.yml logs -f

4. Probar API
   └── http://localhost:5000/swagger

5. Al terminar
   └── docker-compose -f docker-compose.dev.yml down
```

---

**Happy coding! 🚀**
