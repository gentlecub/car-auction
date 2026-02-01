# DOCKER-COMPOSE.md — Orquestación Completa

## Arquitectura de Servicios

```
┌─────────────────────────────────────────────────────────────────┐
│                        NGINX (80/443)                           │
│                     Reverse Proxy + SSL                         │
└──────────────────┬─────────────────────┬────────────────────────┘
                   │                     │
         ┌─────────▼──────┐    ┌─────────▼──────┐
         │   Frontend     │    │     API        │
         │   (React)      │    │  (ASP.NET)     │
         │   :3000        │    │   :5000        │
         └────────────────┘    └───────┬────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
              ┌─────▼─────┐     ┌──────▼─────┐    ┌──────▼─────┐
              │   MySQL   │     │   Redis    │    │  (Future)  │
              │   :3306   │     │   :6379    │    │  RabbitMQ  │
              └───────────┘     └────────────┘    └────────────┘
```

---

## docker-compose.yml (Producción)

```yaml
# /docker-compose.yml (raíz del proyecto)
version: '3.8'

services:
  # ============================================
  # Frontend - React + Nginx
  # ============================================
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
      args:
        VITE_API_URL: ${VITE_API_URL:-http://localhost:5000/api}
        VITE_WS_URL: ${VITE_WS_URL:-ws://localhost:5000/hubs/auction}
    container_name: carauction-frontend
    ports:
      - "3000:80"
    depends_on:
      api:
        condition: service_healthy
    networks:
      - carauction-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "wget", "-q", "--spider", "http://localhost:80/health"]
      interval: 30s
      timeout: 5s
      retries: 3

  # ============================================
  # API Backend - ASP.NET Core 8
  # ============================================
  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: carauction-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
      - ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=carauction;User=carauction;Password=${MYSQL_PASSWORD};
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - JwtSettings__Issuer=${JWT_ISSUER:-CarAuction}
      - JwtSettings__Audience=${JWT_AUDIENCE:-CarAuctionClient}
      - Redis__ConnectionString=redis:6379
      - Cors__Origins=${CORS_ORIGINS:-http://localhost:3000}
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
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
      MYSQL_DATABASE: carauction
      MYSQL_USER: carauction
      MYSQL_PASSWORD: ${MYSQL_PASSWORD}
    volumes:
      - mysql-data:/var/lib/mysql
      - ./backend/scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql:ro
    networks:
      - carauction-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s

  # ============================================
  # Redis Cache
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
    command: redis-server --appendonly yes --maxmemory 256mb

  # ============================================
  # Nginx Reverse Proxy (Profile: production)
  # ============================================
  nginx:
    image: nginx:alpine
    container_name: carauction-proxy
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.prod.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    depends_on:
      - frontend
      - api
    networks:
      - carauction-network
    restart: unless-stopped
    profiles:
      - production

volumes:
  mysql-data:
  redis-data:

networks:
  carauction-network:
    driver: bridge
```

---

## docker-compose.dev.yml (Desarrollo)

```yaml
# /docker-compose.dev.yml
version: '3.8'

services:
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
    volumes:
      - ./frontend/src:/app/src:ro
      - ./frontend/public:/app/public:ro
    ports:
      - "5173:5173"
    environment:
      - VITE_API_URL=http://localhost:5000/api

  api:
    build:
      context: ./backend
      dockerfile: Dockerfile.dev
    volumes:
      - ./backend/src:/src/src:ro
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  mysql:
    image: mysql:8.0
    ports:
      - "3306:3306"
    environment:
      MYSQL_ROOT_PASSWORD: devroot123
      MYSQL_DATABASE: carauction
      MYSQL_USER: carauction
      MYSQL_PASSWORD: devpass123

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

networks:
  default:
    name: carauction-dev
```

---

## Comandos Esenciales

```bash
# Desarrollo
docker-compose -f docker-compose.dev.yml up -d

# Producción (sin proxy)
docker-compose up -d

# Producción (con Nginx proxy)
docker-compose --profile production up -d

# Ver logs
docker-compose logs -f api frontend

# Rebuild específico
docker-compose up -d --build api
```

---

**🛑 CONTINÚA leyendo para Módulo 2.4: Variables y Secretos**
