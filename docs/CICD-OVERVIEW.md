# CICD-OVERVIEW.md — Arquitectura del Pipeline

## Estado Actual

| Componente | CI | CD | Estado |
|------------|----|----|--------|
| Backend (.NET) | ✅ Completo | ✅ Completo | Productivo |
| Frontend (React) | ❌ Faltante | ❌ Faltante | Pendiente |
| Full-Stack | ❌ Faltante | ❌ Faltante | Pendiente |

---

## Arquitectura del Pipeline

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          GITHUB ACTIONS                                  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐         ┌─────────────────┐         ┌─────────────────┐
│   CI Pipeline │         │   CI Pipeline   │         │   CD Pipeline   │
│   (Backend)   │         │   (Frontend)    │         │  (Full-Stack)   │
└───────┬───────┘         └────────┬────────┘         └────────┬────────┘
        │                          │                           │
        ▼                          ▼                           ▼
┌───────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ • Restore     │         │ • npm install   │         │ • Build images  │
│ • Build       │         │ • Lint          │         │ • Push registry │
│ • Unit Tests  │         │ • Type check    │         │ • Deploy stage  │
│ • Int. Tests  │         │ • Unit Tests    │         │ • Health check  │
│ • Coverage    │         │ • Build         │         │ • Deploy prod   │
│ • Docker      │         │ • Docker        │         │ • Notify        │
│ • Security    │         │ • Security      │         │ • Rollback      │
└───────────────┘         └─────────────────┘         └─────────────────┘
```

---

## Triggers por Rama

| Rama | CI Backend | CI Frontend | CD Staging | CD Production |
|------|------------|-------------|------------|---------------|
| `feature/*` | PR only | PR only | ❌ | ❌ |
| `develop` | ✅ Push + PR | ✅ Push + PR | ❌ | ❌ |
| `main` | ✅ Push | ✅ Push | ✅ Auto | 🔘 Manual |

---

## Workflows Existentes (Backend)

### `/backend/.github/workflows/ci.yml`

```yaml
Jobs:
├── build-and-test     # Build + Unit/Integration tests
├── code-quality       # Format check
├── docker-build       # Validar Dockerfile
└── security-scan      # Vulnerabilidades NuGet
```

### `/backend/.github/workflows/cd.yml`

```yaml
Jobs:
├── build-and-push     # GHCR push
├── deploy-staging     # SSH + docker-compose
└── deploy-production  # Manual + backup DB
```

---

## Secrets Requeridos en GitHub

| Secret | Uso | Scope |
|--------|-----|-------|
| `STAGING_HOST` | IP/hostname staging | CD |
| `STAGING_USER` | SSH username | CD |
| `STAGING_SSH_KEY` | Private key | CD |
| `PROD_HOST` | IP/hostname prod | CD |
| `PROD_USER` | SSH username | CD |
| `PROD_SSH_KEY` | Private key | CD |
| `CODECOV_TOKEN` | Coverage reports | CI |
| `DOCKERHUB_USERNAME` | (Opcional) DockerHub | CD |
| `DOCKERHUB_TOKEN` | (Opcional) DockerHub | CD |

---

## Estructura de Archivos Propuesta

```
/.github/
├── workflows/
│   ├── ci-backend.yml       # CI solo backend
│   ├── ci-frontend.yml      # CI solo frontend (NUEVO)
│   ├── cd-deploy.yml        # CD unificado
│   └── security-scan.yml    # SAST/DAST separado
├── actions/
│   └── setup-project/       # Composite action reutilizable
└── dependabot.yml           # Actualizaciones automáticas
```

---

## Flujo de Trabajo Recomendado

```
1. Developer push → feature/xyz
         │
         ▼
2. CI runs (lint, test, build)
         │
         ▼
3. PR to develop → Code review + CI green
         │
         ▼
4. Merge to develop → CI + optional staging preview
         │
         ▼
5. PR to main → Final review
         │
         ▼
6. Merge to main → CD auto-deploy staging
         │
         ▼
7. Manual approval → CD deploy production
```

---

**🛑 CONTINÚA leyendo para Módulo 3.2: CI Frontend**
