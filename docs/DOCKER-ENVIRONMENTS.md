# DOCKER-ENVIRONMENTS.md — Estrategia Dev/Stage/Prod

## Matriz de Entornos

| Aspecto | Development | Staging | Production |
|---------|-------------|---------|------------|
| **Base de datos** | Local MySQL | Cloud MySQL (réplica) | Cloud MySQL (HA) |
| **Redis** | Local single | Cloud single | Cloud cluster |
| **SSL/TLS** | No | Let's Encrypt | Certificado comercial |
| **Logs** | Console | Structured JSON | ELK/CloudWatch |
| **Debug** | Habilitado | Limitado | Deshabilitado |
| **Hot Reload** | Sí | No | No |
| **Réplicas** | 1 | 2 | 3+ (auto-scaling) |

---

## Estructura de Archivos

```
/
├── docker-compose.yml           # Base común
├── docker-compose.dev.yml       # Override desarrollo
├── docker-compose.staging.yml   # Override staging
├── docker-compose.prod.yml      # Override producción
├── .env.example                 # Template
├── .env.development             # Variables dev
├── .env.staging                 # Variables staging
└── .env.production              # Variables prod
```

---

## docker-compose.staging.yml

```yaml
version: '3.8'

services:
  frontend:
    build:
      args:
        VITE_API_URL: https://staging-api.carauction.com/api
        VITE_WS_URL: wss://staging-api.carauction.com/hubs/auction
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '0.5'
          memory: 512M

  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Staging
      - Logging__LogLevel__Default=Debug
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1'
          memory: 1G

  mysql:
    # En staging: usar servicio cloud externo
    # Comentar si usas RDS/Cloud SQL
    deploy:
      resources:
        limits:
          memory: 2G
```

---

## docker-compose.prod.yml

```yaml
version: '3.8'

services:
  frontend:
    build:
      args:
        VITE_API_URL: https://api.carauction.com/api
        VITE_WS_URL: wss://api.carauction.com/hubs/auction
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
      resources:
        limits:
          cpus: '0.5'
          memory: 256M

  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Logging__LogLevel__Default=Warning
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 30s
        failure_action: rollback
      resources:
        limits:
          cpus: '2'
          memory: 2G
    logging:
      driver: json-file
      options:
        max-size: "10m"
        max-file: "5"

  nginx:
    profiles: []  # Activo por defecto en prod
    volumes:
      - ./nginx/ssl:/etc/nginx/ssl:ro
      - ./nginx/nginx.prod.conf:/etc/nginx/nginx.conf:ro
```

---

## Comandos por Entorno

```bash
# ============================================
# DESARROLLO
# ============================================
docker-compose -f docker-compose.yml \
               -f docker-compose.dev.yml \
               --env-file .env.development \
               up -d

# ============================================
# STAGING
# ============================================
docker-compose -f docker-compose.yml \
               -f docker-compose.staging.yml \
               --env-file .env.staging \
               up -d

# ============================================
# PRODUCCIÓN
# ============================================
docker-compose -f docker-compose.yml \
               -f docker-compose.prod.yml \
               --env-file .env.production \
               up -d

# Con Swarm (producción escalable)
docker stack deploy -c docker-compose.yml \
                    -c docker-compose.prod.yml \
                    carauction
```

---

## Makefile para Simplificar

```makefile
# /Makefile
.PHONY: dev staging prod down logs

dev:
	docker-compose -f docker-compose.yml -f docker-compose.dev.yml \
	--env-file .env.development up -d

staging:
	docker-compose -f docker-compose.yml -f docker-compose.staging.yml \
	--env-file .env.staging up -d

prod:
	docker-compose -f docker-compose.yml -f docker-compose.prod.yml \
	--env-file .env.production up -d

down:
	docker-compose down -v

logs:
	docker-compose logs -f

build:
	docker-compose build --no-cache

clean:
	docker system prune -af
```

---

## Validación de Entorno

```bash
# Verificar configuración sin ejecutar
docker-compose -f docker-compose.yml \
               -f docker-compose.prod.yml \
               --env-file .env.production \
               config

# Verificar variables cargadas
docker-compose config | grep -E "(ASPNETCORE|JWT|MYSQL)"
```

---

**FASE 2 COMPLETADA**

---

**🛑 DETENTE — Escribe `CONTINUAR` para FASE 3: CI/CD Pipeline**
