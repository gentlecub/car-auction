# Despliegue en Railway.app - Car Auction

Esta guía te explica paso a paso cómo desplegar tu aplicación Car Auction en Railway.app para tener una demo pública accesible desde cualquier lugar.

## Arquitectura en Railway

```
Railway Project: car-auction-demo
├── backend (ASP.NET Core API)     → https://backend-xxx.up.railway.app
├── frontend (React + Nginx)       → https://frontend-xxx.up.railway.app
├── MySQL (Plugin)                 → Conexión interna
└── Redis (Plugin)                 → Conexión interna
```

## Requisitos Previos

1. Cuenta en [Railway.app](https://railway.app) (puedes usar GitHub para registrarte)
2. Repositorio de GitHub con tu código
3. Los archivos de configuración ya creados:
   - `backend/railway.toml`
   - `backend/Dockerfile.railway`
   - `frontend/railway.toml`
   - `frontend/Dockerfile.railway`
   - `frontend/nginx/nginx.railway.conf`

---

## Paso 1: Preparar el Repositorio

### 1.1 Subir cambios a GitHub

```bash
# Asegúrate de que todos los archivos de Railway estén commiteados
git add .
git commit -m "Add Railway deployment configuration"
git push origin main
```

### 1.2 Verificar estructura del repositorio

Tu repositorio debe tener esta estructura:

```
car-auction/
├── backend/
│   ├── Dockerfile.railway      # Dockerfile para Railway
│   ├── railway.toml            # Configuración de Railway
│   └── src/
│       └── CarAuction.API/
├── frontend/
│   ├── Dockerfile.railway      # Dockerfile para Railway
│   ├── railway.toml            # Configuración de Railway
│   └── nginx/
│       └── nginx.railway.conf  # Nginx para Railway
└── docs/
    └── RAILWAY-DEPLOY.md       # Esta guía
```

---

## Paso 2: Crear Proyecto en Railway

### 2.1 Iniciar sesión en Railway

1. Ve a [railway.app](https://railway.app)
2. Haz clic en **"Start a New Project"** o **"New Project"**
3. Selecciona **"Deploy from GitHub repo"**
4. Autoriza Railway para acceder a tu cuenta de GitHub si es necesario

### 2.2 Seleccionar repositorio

1. Busca tu repositorio `car-auction`
2. Haz clic en él para seleccionarlo
3. Railway detectará automáticamente tu proyecto

### 2.3 Configuración inicial

**IMPORTANTE**: No despliegues todavía. Primero necesitamos agregar los servicios de base de datos.

1. Haz clic en **"Configure"** o cancela el despliegue automático
2. Renombra el proyecto a algo como `car-auction-demo`

---

## Paso 3: Agregar MySQL (Plugin)

### 3.1 Agregar servicio MySQL

1. En tu proyecto Railway, haz clic en **"+ New"** → **"Database"** → **"MySQL"**
2. Railway creará automáticamente una instancia de MySQL
3. Espera a que el servicio esté activo (icono verde)

### 3.2 Obtener credenciales de MySQL

1. Haz clic en el servicio **MySQL**
2. Ve a la pestaña **"Variables"**
3. Verás las variables automáticas:
   - `MYSQL_URL` (URL completa)
   - `MYSQL_HOST`
   - `MYSQL_PORT`
   - `MYSQL_DATABASE`
   - `MYSQL_USER`
   - `MYSQL_PASSWORD`

**Guarda estas variables**, las necesitarás para el backend.

---

## Paso 4: Agregar Redis (Plugin)

### 4.1 Agregar servicio Redis

1. Haz clic en **"+ New"** → **"Database"** → **"Redis"**
2. Railway creará automáticamente una instancia de Redis
3. Espera a que el servicio esté activo

### 4.2 Obtener URL de Redis

1. Haz clic en el servicio **Redis**
2. Ve a la pestaña **"Variables"**
3. Copia la variable `REDIS_URL`

---

## Paso 5: Desplegar Backend

### 5.1 Crear servicio del Backend

1. Haz clic en **"+ New"** → **"GitHub Repo"**
2. Selecciona tu repositorio `car-auction`
3. En **"Root Directory"**, escribe: `backend`
4. Railway detectará el `railway.toml` y usará `Dockerfile.railway`

### 5.2 Configurar variables de entorno del Backend

1. Haz clic en el servicio **backend**
2. Ve a **"Variables"**
3. Agrega las siguientes variables:

```env
# Conexión a MySQL (referencia las variables del plugin MySQL)
ConnectionStrings__DefaultConnection=Server=${MYSQL_HOST};Port=${MYSQL_PORT};Database=${MYSQL_DATABASE};User=${MYSQL_USER};Password=${MYSQL_PASSWORD};

# Redis
Redis__ConnectionString=${REDIS_URL}

# JWT (genera una clave segura de al menos 32 caracteres)
JwtSettings__SecretKey=TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!
JwtSettings__Issuer=CarAuction
JwtSettings__Audience=CarAuctionClient
JwtSettings__AccessTokenExpirationMinutes=60
JwtSettings__RefreshTokenExpirationDays=7

# Habilitar Swagger para la demo
ENABLE_SWAGGER_IN_PRODUCTION=true

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
```

### 5.3 Vincular variables de MySQL y Redis

Railway permite referenciar variables de otros servicios:

1. En las variables del backend, usa la sintaxis `${SERVICE_VARIABLE}`
2. Haz clic en **"Add Variable Reference"**
3. Selecciona el servicio MySQL y las variables que necesitas
4. Repite para Redis

### 5.4 Exponer el Backend públicamente

1. En el servicio backend, ve a **"Settings"**
2. En **"Networking"**, haz clic en **"Generate Domain"**
3. Railway generará una URL como: `https://car-auction-backend-production.up.railway.app`
4. **Copia esta URL**, la necesitas para el frontend y CORS

### 5.5 Actualizar CORS

Vuelve a las variables del backend y agrega:

```env
Cors__Origins__0=https://TU-FRONTEND-URL.up.railway.app
```

(Actualizarás esto después de crear el frontend)

### 5.6 Desplegar

1. Haz clic en **"Deploy"** o espera el despliegue automático
2. Verifica en **"Deployments"** que el build sea exitoso
3. Prueba accediendo a `https://tu-backend.up.railway.app/swagger`

---

## Paso 6: Desplegar Frontend

### 6.1 Crear servicio del Frontend

1. Haz clic en **"+ New"** → **"GitHub Repo"**
2. Selecciona tu repositorio `car-auction`
3. En **"Root Directory"**, escribe: `frontend`
4. Railway detectará el `railway.toml` y usará `Dockerfile.railway`

### 6.2 Configurar variables de entorno del Frontend

1. Haz clic en el servicio **frontend**
2. Ve a **"Variables"**
3. Agrega las siguientes variables de **BUILD** (importantes para Vite):

```env
# URL del API (la URL de tu backend)
VITE_API_URL=https://tu-backend.up.railway.app/api

# URL de WebSocket para SignalR
VITE_WS_URL=wss://tu-backend.up.railway.app/hubs/auction

# Ambiente
VITE_ENV=production
```

**IMPORTANTE**: Reemplaza `tu-backend.up.railway.app` con la URL real de tu backend.

### 6.3 Exponer el Frontend públicamente

1. En el servicio frontend, ve a **"Settings"**
2. En **"Networking"**, haz clic en **"Generate Domain"**
3. Railway generará una URL como: `https://car-auction-frontend-production.up.railway.app`

### 6.4 Actualizar CORS del Backend

Ahora que tienes la URL del frontend:

1. Ve al servicio **backend** → **Variables**
2. Actualiza:
   ```env
   Cors__Origins__0=https://tu-frontend.up.railway.app
   ```
3. El backend se redespleará automáticamente

### 6.5 Desplegar Frontend

1. Haz clic en **"Deploy"**
2. Espera a que el build termine (puede tomar 2-3 minutos)
3. Una vez completo, accede a tu URL del frontend

---

## Paso 7: Verificar el Despliegue

### 7.1 Verificar Backend

1. **Health Check**: `https://tu-backend.up.railway.app/health`
   - Debe devolver JSON con status "Healthy"

2. **Swagger**: `https://tu-backend.up.railway.app/swagger`
   - Debe mostrar la documentación de la API

3. **API**: `https://tu-backend.up.railway.app/api/v1/cars`
   - Debe devolver la lista de carros (puede estar vacía inicialmente)

### 7.2 Verificar Frontend

1. Abre `https://tu-frontend.up.railway.app`
2. Deberías ver la página de inicio de Car Auction
3. Prueba navegar entre las páginas
4. Verifica que las llamadas a la API funcionen (ver Network en DevTools)

### 7.3 Verificar Conectividad

1. Abre DevTools del navegador (F12)
2. Ve a la pestaña **Network**
3. Las peticiones a `/api/*` deben responder correctamente
4. No debe haber errores de CORS

---

## Paso 8: Configuración Adicional (Opcional)

### 8.1 Dominio Personalizado

Railway permite agregar dominios personalizados:

1. Ve a **Settings** del servicio
2. En **"Custom Domain"**, agrega tu dominio
3. Configura el DNS de tu dominio apuntando al CNAME de Railway

### 8.2 Variables de Entorno Compartidas

Para compartir variables entre servicios:

1. Ve a **Project Settings** → **Shared Variables**
2. Agrega variables que necesiten ambos servicios

### 8.3 Monitoreo

1. En cada servicio, ve a **"Metrics"** para ver uso de recursos
2. Ve a **"Logs"** para ver logs en tiempo real
3. Configura alertas en **Project Settings**

---

## Troubleshooting

### Error: "Port already in use"

Railway asigna el puerto automáticamente via `$PORT`. Verifica que tu Dockerfile use:
```dockerfile
ENV PORT=5000
# Y el CMD use: ASPNETCORE_URLS=http://+:${PORT}
```

### Error: "Connection refused" a MySQL

1. Verifica que las variables de MySQL estén correctamente referenciadas
2. El formato de conexión debe ser:
   ```
   Server=${MYSQL_HOST};Port=${MYSQL_PORT};Database=${MYSQL_DATABASE};User=${MYSQL_USER};Password=${MYSQL_PASSWORD};
   ```

### Error: CORS

1. Verifica que `Cors__Origins__0` tenga la URL exacta del frontend
2. La URL debe incluir `https://` y no tener `/` al final
3. Redespliega el backend después de cambiar CORS

### Error: Swagger no aparece

1. Verifica que `ENABLE_SWAGGER_IN_PRODUCTION=true` esté configurado
2. El backend debe tener esta variable antes de construir

### Build falla en Frontend

1. Verifica que las variables `VITE_*` estén configuradas
2. Estas variables deben existir **durante el build**, no solo en runtime
3. Haz un nuevo deploy si agregaste variables después del primer build

### WebSocket/SignalR no conecta

1. Verifica que `VITE_WS_URL` use `wss://` (no `ws://`)
2. Railway soporta WebSockets automáticamente en HTTPS

---

## URLs Finales para tu Portafolio

Una vez desplegado, tendrás estas URLs:

| Servicio | URL |
|----------|-----|
| Frontend | `https://car-auction-frontend-xxx.up.railway.app` |
| API | `https://car-auction-backend-xxx.up.railway.app/api` |
| Swagger | `https://car-auction-backend-xxx.up.railway.app/swagger` |
| Health | `https://car-auction-backend-xxx.up.railway.app/health` |

### Ejemplo de cómo presentarlo en tu portafolio:

```markdown
## Car Auction - Live Demo

- **Demo**: [car-auction.up.railway.app](https://tu-frontend.up.railway.app)
- **API Docs**: [Swagger UI](https://tu-backend.up.railway.app/swagger)

Tecnologías: React, ASP.NET Core 8, MySQL, Redis, Docker, SignalR
```

---

## Costos

Railway ofrece:
- **Free Tier**: $5 USD de crédito gratis al mes
- **Hobby Plan**: $5 USD/mes con recursos adicionales

Para una demo pequeña, el Free Tier debería ser suficiente.

---

## Comandos Útiles

```bash
# Instalar Railway CLI
npm i -g @railway/cli

# Login
railway login

# Ver logs del proyecto
railway logs

# Variables de entorno
railway variables

# Desplegar manualmente
railway up
```

---

## Soporte

- [Documentación de Railway](https://docs.railway.app/)
- [Comunidad de Railway](https://discord.gg/railway)
- [Status de Railway](https://status.railway.app/)
