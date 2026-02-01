# DOCKER-FRONTEND.md — Dockerfile React + Vite

## Arquitectura Multi-Stage

```
┌─────────────────────────────────────────────────────────────┐
│  Stage 1: deps        │  Instala dependencias (cacheable)   │
├───────────────────────┼─────────────────────────────────────┤
│  Stage 2: builder     │  Compila aplicación Vite            │
├───────────────────────┼─────────────────────────────────────┤
│  Stage 3: runner      │  Nginx sirve archivos estáticos     │
└─────────────────────────────────────────────────────────────┘
```

---

## Dockerfile Producción

```dockerfile
# frontend/Dockerfile
# ============================================
# CarAuction Frontend - Production Dockerfile
# Multi-stage build optimizado para Vite + React
# ============================================

# Stage 1: Dependencies
FROM node:20-alpine AS deps
WORKDIR /app

# Instalar solo dependencias (cacheo eficiente)
COPY package.json package-lock.json* ./
RUN npm ci --only=production=false

# Stage 2: Builder
FROM node:20-alpine AS builder
WORKDIR /app

# Copiar dependencias del stage anterior
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build arguments para variables de entorno en build time
ARG VITE_API_URL
ARG VITE_WS_URL
ARG VITE_ENV=production

ENV VITE_API_URL=$VITE_API_URL
ENV VITE_WS_URL=$VITE_WS_URL
ENV VITE_ENV=$VITE_ENV

# Ejecutar build
RUN npm run build

# Stage 3: Runner (Nginx)
FROM nginx:alpine AS runner

# Copiar configuración nginx personalizada
COPY nginx/nginx.conf /etc/nginx/nginx.conf

# Copiar archivos compilados
COPY --from=builder /app/dist /usr/share/nginx/html

# Usuario no-root para seguridad
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

USER nginx

# Puerto
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:80/health || exit 1

CMD ["nginx", "-g", "daemon off;"]
```

---

## Nginx Config para SPA

```nginx
# frontend/nginx/nginx.conf
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Logs
    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';
    access_log /var/log/nginx/access.log main;

    # Performance
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    gzip on;
    gzip_types text/plain text/css application/json application/javascript;

    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;

        # Health check endpoint
        location /health {
            access_log off;
            return 200 "healthy\n";
            add_header Content-Type text/plain;
        }

        # Servir archivos estáticos con cache
        location /assets {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }

        # SPA fallback - todas las rutas a index.html
        location / {
            try_files $uri $uri/ /index.html;
        }

        # Security headers
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
    }
}
```

---

## Dockerfile Desarrollo

```dockerfile
# frontend/Dockerfile.dev
FROM node:20-alpine

WORKDIR /app

# Instalar dependencias
COPY package.json package-lock.json* ./
RUN npm ci

# Copiar código fuente
COPY . .

# Puerto Vite dev server
EXPOSE 5173

# Hot reload habilitado
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
```

---

## .dockerignore Frontend

```
# frontend/.dockerignore
node_modules
dist
.git
.gitignore
*.md
.env*.local
.vscode
.idea
coverage
```

---

## Comandos de Build

```bash
# Build producción
docker build -t carauction-frontend:latest \
  --build-arg VITE_API_URL=https://api.carauction.com/api \
  --build-arg VITE_WS_URL=wss://api.carauction.com/hubs/auction \
  ./frontend

# Build desarrollo
docker build -f Dockerfile.dev -t carauction-frontend:dev ./frontend

# Ejecutar contenedor
docker run -d -p 3000:80 carauction-frontend:latest
```

---

**Imagen final: ~25MB** (nginx:alpine + archivos estáticos)

---

**🛑 CONTINÚA leyendo para Módulo 2.2: Docker Backend**
