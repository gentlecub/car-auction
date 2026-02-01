# INTEGRATION.md — Arquitectura de Comunicación Frontend ↔ Backend

## 1. Flujo de Comunicación

```
┌─────────────────┐     HTTPS/WSS      ┌─────────────────┐     TCP      ┌─────────────┐
│   React App     │ ◄─────────────────► │  ASP.NET Core   │ ◄──────────► │   MySQL     │
│   (Vite:5173)   │      REST/JWT       │  API (:5000)    │              │   (:3306)   │
└─────────────────┘                     └────────┬────────┘              └─────────────┘
                                                 │
                                    SignalR Hub  │  Cache
                                                 ▼
                                        ┌─────────────────┐
                                        │     Redis       │
                                        │    (:6379)      │
                                        └─────────────────┘
```

## 2. Endpoints API Disponibles

| Controller       | Base Route            | Descripción                    |
|------------------|-----------------------|--------------------------------|
| AuthController   | `/api/auth`           | Login, Register, Refresh Token |
| CarsController   | `/api/cars`           | CRUD vehículos                 |
| AuctionsController| `/api/auctions`      | Gestión subastas               |
| BidsController   | `/api/bids`           | Crear/consultar pujas          |
| UsersController  | `/api/users`          | Perfil usuario                 |
| AdminController  | `/api/admin`          | Panel administración           |
| NotificationsController | `/api/notifications` | Notificaciones usuario    |

**SignalR Hub**: `ws://localhost:5000/hubs/auction` — Real-time bidding

---

## 3. Estructura de Servicios API en Frontend

```
frontend/src/
├── services/
│   ├── api/
│   │   ├── axiosInstance.ts      # Configuración base Axios
│   │   ├── authService.ts        # Login, logout, refresh
│   │   ├── carService.ts         # CRUD vehículos
│   │   ├── auctionService.ts     # Gestión subastas
│   │   ├── bidService.ts         # Pujas
│   │   └── userService.ts        # Perfil
│   ├── hooks/
│   │   ├── useAuth.ts            # Hook autenticación
│   │   └── useAuctionHub.ts      # Hook SignalR
│   └── context/
│       └── AuthContext.tsx       # Estado global auth
```

---

## 4. Configuración Axios Instance

```typescript
// frontend/src/services/api/axiosInstance.ts
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const axiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
});

// Interceptor: Agregar JWT a cada request
axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor: Manejar 401 y refresh token
axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Intentar refresh token o redirigir a login
      localStorage.removeItem('accessToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
```

---

## 5. Variables de Entorno Frontend

```env
# frontend/.env.development
VITE_API_URL=http://localhost:5000/api
VITE_WS_URL=ws://localhost:5000/hubs/auction
VITE_ENV=development

# frontend/.env.production
VITE_API_URL=https://api.carauction.com/api
VITE_WS_URL=wss://api.carauction.com/hubs/auction
VITE_ENV=production
```

---

## 6. Configuración CORS en Backend

El backend ya tiene CORS configurado via `Cors__Origins`. Verificar en `Program.cs`:

```csharp
// Orígenes permitidos desde variable de entorno
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = Environment.GetEnvironmentVariable("CORS_ORIGINS")
            ?.Split(',') ?? new[] { "http://localhost:5173" };

        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Requerido para SignalR
    });
});
```

---

## 7. Checklist de Conexión Exitosa

| # | Verificación                                    | Comando/Acción                          |
|---|------------------------------------------------|----------------------------------------|
| 1 | Backend responde                               | `curl http://localhost:5000/health`    |
| 2 | CORS permite origen frontend                   | Verificar headers `Access-Control-*`   |
| 3 | Login retorna JWT                              | POST `/api/auth/login`                 |
| 4 | Token se envía en header                       | `Authorization: Bearer <token>`        |
| 5 | Endpoint protegido responde 200                | GET `/api/users/me` con token          |
| 6 | SignalR conecta                                | WebSocket connection established       |
| 7 | Variables .env cargadas                        | `console.log(import.meta.env)`         |

---

## 8. Recomendaciones de Seguridad

- **NUNCA** hardcodear tokens o secrets en código
- Usar `HttpOnly` cookies para refresh tokens (más seguro que localStorage)
- Implementar CSRF protection si se usan cookies
- Validar **siempre** en backend, nunca confiar solo en frontend
- Rate limiting en endpoints de autenticación
- HTTPS obligatorio en producción

---

## 9. Dependencias Requeridas Frontend

```bash
cd frontend
npm install axios @microsoft/signalr
```

---

**🛑 DETENTE — Espera "CONTINUAR" para FASE 2: Dockerización**
