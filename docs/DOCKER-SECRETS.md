# DOCKER-SECRETS.md — Gestión de Variables y Secretos

## Estrategia de Seguridad

```
┌─────────────────────────────────────────────────────────────┐
│                    NUNCA EN CÓDIGO                          │
│  ❌ Hardcoded passwords  ❌ API keys  ❌ JWT secrets        │
└─────────────────────────────────────────────────────────────┘
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
       ┌──────────┐  ┌───────────┐  ┌───────────────┐
       │  .env    │  │  Docker   │  │  Cloud        │
       │  files   │  │  Secrets  │  │  Vault/KMS    │
       │  (dev)   │  │  (swarm)  │  │  (prod)       │
       └──────────┘  └───────────┘  └───────────────┘
```

---

## Archivos de Entorno

### .env.example (Template - SÍ commitear)

```env
# /.env.example
# ============================================
# CarAuction - Environment Template
# Copy to .env and fill values
# ============================================

# Database
MYSQL_ROOT_PASSWORD=
MYSQL_PASSWORD=

# JWT (generate: openssl rand -base64 32)
JWT_SECRET_KEY=
JWT_ISSUER=CarAuction
JWT_AUDIENCE=CarAuctionClient
JWT_ACCESS_EXPIRATION=60
JWT_REFRESH_EXPIRATION=7

# CORS
CORS_ORIGINS=http://localhost:3000,http://localhost:5173

# Frontend Build
VITE_API_URL=http://localhost:5000/api
VITE_WS_URL=ws://localhost:5000/hubs/auction

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### .env.development (NO commitear)

```env
# /.env.development
MYSQL_ROOT_PASSWORD=dev_root_123
MYSQL_PASSWORD=dev_pass_123
JWT_SECRET_KEY=dev-secret-key-minimum-32-characters-here
CORS_ORIGINS=http://localhost:3000,http://localhost:5173
VITE_API_URL=http://localhost:5000/api
ASPNETCORE_ENVIRONMENT=Development
```

### .env.production (NO commitear)

```env
# /.env.production
MYSQL_ROOT_PASSWORD=${GENERATED_SECURE_PASSWORD}
MYSQL_PASSWORD=${GENERATED_SECURE_PASSWORD}
JWT_SECRET_KEY=${GENERATED_64_CHAR_SECRET}
CORS_ORIGINS=https://carauction.com,https://www.carauction.com
VITE_API_URL=https://api.carauction.com/api
VITE_WS_URL=wss://api.carauction.com/hubs/auction
ASPNETCORE_ENVIRONMENT=Production
```

---

## .gitignore para Secretos

```gitignore
# Secrets - NUNCA commitear
.env
.env.local
.env.development
.env.staging
.env.production
*.pem
*.key
**/appsettings.*.json
!**/appsettings.json
!**/appsettings.Development.json.example
```

---

## Docker Secrets (Swarm Mode)

```yaml
# docker-compose.secrets.yml
version: '3.8'

services:
  api:
    secrets:
      - jwt_secret
      - mysql_password
    environment:
      - JwtSettings__SecretKey_FILE=/run/secrets/jwt_secret
      - MYSQL_PASSWORD_FILE=/run/secrets/mysql_password

secrets:
  jwt_secret:
    external: true
  mysql_password:
    external: true
```

```bash
# Crear secretos en Docker Swarm
echo "super-secure-jwt-key-here" | docker secret create jwt_secret -
echo "mysql-secure-password" | docker secret create mysql_password -
```

---

## Generación de Secretos Seguros

```bash
# JWT Secret (64 caracteres)
openssl rand -base64 48

# Database password
openssl rand -base64 24

# API Key
openssl rand -hex 32
```

---

## Integración con Cloud Secrets

### AWS Secrets Manager

```csharp
// Program.cs
builder.Configuration.AddSecretsManager(region: "us-east-1");
```

### Azure Key Vault

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### HashiCorp Vault

```yaml
# docker-compose con Vault
services:
  api:
    environment:
      VAULT_ADDR: http://vault:8200
      VAULT_TOKEN: ${VAULT_TOKEN}
```

---

## Checklist de Seguridad

| # | Verificación | Estado |
|---|--------------|--------|
| 1 | `.env` en `.gitignore` | ⬜ |
| 2 | Secretos ≥32 caracteres | ⬜ |
| 3 | Diferentes secretos por entorno | ⬜ |
| 4 | No hay secretos en Dockerfile | ⬜ |
| 5 | Variables sensibles no en logs | ⬜ |
| 6 | Rotación de secretos planificada | ⬜ |

---

**🛑 CONTINÚA leyendo para Módulo 2.5: Estrategia de Entornos**
