# QA-CHECKLIST.md — Checklist de Calidad Enterprise

## 1. Código y Arquitectura

### Backend (.NET Core 8)

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 1.1 | Clean Architecture | Capas Domain/Application/Infrastructure separadas | ⬜ |
| 1.2 | Dependency Injection | Todos los servicios inyectados via DI | ⬜ |
| 1.3 | Repository Pattern | Abstracción de acceso a datos | ⬜ |
| 1.4 | DTOs | No exponer entidades de dominio en API | ⬜ |
| 1.5 | Validation | FluentValidation en DTOs de entrada | ⬜ |
| 1.6 | Error Handling | ExceptionMiddleware centralizado | ⬜ |
| 1.7 | Logging | Structured logging (Serilog) | ⬜ |
| 1.8 | API Versioning | `/api/v1/` implementado | ⬜ |

### Frontend (React)

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 1.9 | Component Structure | Atomic design o feature-based | ⬜ |
| 1.10 | State Management | Context/Zustand para estado global | ⬜ |
| 1.11 | Type Safety | TypeScript strict mode | ⬜ |
| 1.12 | Error Boundaries | Manejo de errores en componentes | ⬜ |
| 1.13 | Loading States | Skeleton/Spinners consistentes | ⬜ |
| 1.14 | Form Validation | react-hook-form + validación | ⬜ |

---

## 2. Seguridad

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 2.1 | JWT Implementation | Access + Refresh tokens | ⬜ |
| 2.2 | Password Hashing | BCrypt/Argon2 (no MD5/SHA1) | ⬜ |
| 2.3 | CORS Configured | Orígenes específicos, no wildcard | ⬜ |
| 2.4 | HTTPS Only | Redirect HTTP → HTTPS | ⬜ |
| 2.5 | Security Headers | X-Frame-Options, CSP, HSTS | ⬜ |
| 2.6 | Input Sanitization | Anti-XSS en inputs | ⬜ |
| 2.7 | SQL Injection | Parámetros en queries (EF Core) | ⬜ |
| 2.8 | Rate Limiting | Throttling en endpoints auth | ⬜ |
| 2.9 | Secrets Management | No hardcoded, usar env vars | ⬜ |
| 2.10 | Audit Logging | Registro de acciones críticas | ⬜ |

---

## 3. Performance

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 3.1 | Database Indexing | Índices en campos de búsqueda | ⬜ |
| 3.2 | Query Optimization | No N+1, usar Include() | ⬜ |
| 3.3 | Caching | Redis para datos frecuentes | ⬜ |
| 3.4 | Pagination | Límite en listados (max 100) | ⬜ |
| 3.5 | Compression | Gzip/Brotli habilitado | ⬜ |
| 3.6 | Bundle Size | Frontend < 500KB inicial | ⬜ |
| 3.7 | Lazy Loading | Code splitting por rutas | ⬜ |
| 3.8 | Image Optimization | WebP, lazy load imágenes | ⬜ |
| 3.9 | CDN | Assets estáticos en CDN | ⬜ |

---

## 4. Testing

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 4.1 | Unit Tests Backend | Coverage ≥ 70% | ⬜ |
| 4.2 | Integration Tests | Endpoints críticos cubiertos | ⬜ |
| 4.3 | Unit Tests Frontend | Componentes principales | ⬜ |
| 4.4 | E2E Tests | Flujos críticos (login, bid) | ⬜ |
| 4.5 | Load Testing | Benchmark con k6/Artillery | ⬜ |

---

## 5. DevOps

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 5.1 | CI Pipeline | Build + Test automático | ⬜ |
| 5.2 | CD Pipeline | Deploy automatizado | ⬜ |
| 5.3 | Docker Multi-stage | Imágenes optimizadas | ⬜ |
| 5.4 | Health Checks | `/health` endpoint | ⬜ |
| 5.5 | Graceful Shutdown | Manejo de SIGTERM | ⬜ |
| 5.6 | Environment Separation | Dev/Staging/Prod aislados | ⬜ |
| 5.7 | Backup Strategy | DB backups automáticos | ⬜ |
| 5.8 | Monitoring | Métricas y alertas | ⬜ |
| 5.9 | Logging Centralized | ELK/CloudWatch/Seq | ⬜ |

---

## 6. Documentación

| # | Criterio | Verificación | Estado |
|---|----------|--------------|--------|
| 6.1 | API Documentation | Swagger/OpenAPI | ⬜ |
| 6.2 | README actualizado | Setup instructions | ⬜ |
| 6.3 | Architecture Docs | Diagramas C4/PlantUML | ⬜ |
| 6.4 | Runbook | Procedimientos operativos | ⬜ |
| 6.5 | CHANGELOG | Versionado semántico | ⬜ |

---

## Comando de Verificación Rápida

```bash
#!/bin/bash
# /scripts/qa-check.sh

echo "=== QA Quick Check ==="

# Backend health
echo -n "Backend health: "
curl -sf http://localhost:5000/health && echo "✅" || echo "❌"

# Frontend accessible
echo -n "Frontend accessible: "
curl -sf http://localhost:3000 > /dev/null && echo "✅" || echo "❌"

# Database connection
echo -n "Database connection: "
docker-compose exec -T mysql mysqladmin ping -h localhost -u root -p${MYSQL_ROOT_PASSWORD} 2>/dev/null && echo "✅" || echo "❌"

# Redis connection
echo -n "Redis connection: "
docker-compose exec -T redis redis-cli ping 2>/dev/null | grep -q PONG && echo "✅" || echo "❌"

# Docker containers
echo -n "All containers running: "
[ $(docker-compose ps -q | wc -l) -ge 4 ] && echo "✅" || echo "❌"

echo "=== Check Complete ==="
```

---

**🛑 CONTINÚA leyendo para Módulo 4.2: Pruebas de Integración**
