# Seguridad y Autenticación

Documentación del sistema de autenticación y autorización del backend CarAuction.

---

## Resumen del Sistema

| Componente | Tecnología | Descripción |
|------------|------------|-------------|
| Autenticación | JWT Bearer | Tokens de acceso stateless |
| Refresh Tokens | Base de datos | Tokens rotativos con revocación |
| Hashing | BCrypt (work factor 12) | Contraseñas seguras |
| Autorización | Role-based (RBAC) | Admin, User |
| Verificación | Email token | Confirmación de cuenta |

---

## Arquitectura de Autenticación

### Flujo de Autenticación Completo

```
┌─────────────────────────────────────────────────────────────────┐
│                     REGISTRO DE USUARIO                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Cliente ──POST /register──▶ AuthController                    │
│                                    │                            │
│                                    ▼                            │
│                            ┌──────────────┐                     │
│                            │ AuthService  │                     │
│                            └──────┬───────┘                     │
│                                   │                             │
│          ┌────────────────────────┼────────────────────────┐    │
│          │                        │                        │    │
│          ▼                        ▼                        ▼    │
│  ┌───────────────┐    ┌─────────────────┐    ┌─────────────┐   │
│  │ Validar email │    │ Hash password   │    │ Crear token │   │
│  │ único         │    │ BCrypt(12)      │    │ verificación│   │
│  └───────────────┘    └─────────────────┘    └─────────────┘   │
│          │                        │                        │    │
│          └────────────────────────┼────────────────────────┘    │
│                                   │                             │
│                                   ▼                             │
│                        ┌─────────────────┐                      │
│                        │ Crear usuario   │                      │
│                        │ + Asignar rol   │                      │
│                        │ "User"          │                      │
│                        └────────┬────────┘                      │
│                                 │                               │
│          ┌──────────────────────┼──────────────────────┐        │
│          │                      │                      │        │
│          ▼                      ▼                      ▼        │
│  ┌───────────────┐    ┌─────────────────┐    ┌─────────────┐   │
│  │ Generar       │    │ Generar         │    │ Enviar email│   │
│  │ AccessToken   │    │ RefreshToken    │    │ verificación│   │
│  │ (JWT)         │    │ (DB)            │    │ (async)     │   │
│  └───────────────┘    └─────────────────┘    └─────────────┘   │
│                                                                 │
│  ◀────────────── AuthResponse ──────────────────────────────── │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Flujo de Login

```
┌─────────────────────────────────────────────────────────────────┐
│                         LOGIN                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Cliente envía credenciales                                  │
│     POST /api/v1/auth/login                                     │
│     { "email": "...", "password": "..." }                       │
│                                                                 │
│  2. Servidor valida:                                            │
│     ├─ Usuario existe?                                          │
│     ├─ BCrypt.Verify(password, hash)?                          │
│     └─ Status == Active?                                        │
│                                                                 │
│  3. Si válido:                                                  │
│     ├─ Actualiza LastLoginAt                                    │
│     ├─ Genera AccessToken (JWT, 60 min)                         │
│     ├─ Genera RefreshToken (DB, 7 días)                         │
│     └─ Registra IP y UserAgent                                  │
│                                                                 │
│  4. Respuesta:                                                  │
│     {                                                           │
│       "accessToken": "eyJhbG...",                              │
│       "refreshToken": "abc123...",                              │
│       "expiresAt": "2024-01-01T12:00:00Z",                     │
│       "user": { ... }                                           │
│     }                                                           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## JSON Web Tokens (JWT)

### Estructura del Access Token

```
Header.Payload.Signature
```

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (Claims):**
```json
{
  "nameid": "123",                    // ClaimTypes.NameIdentifier (User ID)
  "email": "user@example.com",        // ClaimTypes.Email
  "name": "Juan Pérez",               // ClaimTypes.Name (FullName)
  "firstName": "Juan",                // Custom claim
  "lastName": "Pérez",                // Custom claim
  "role": ["User", "Admin"],          // ClaimTypes.Role (array)
  "exp": 1704110400,                  // Expiración (Unix timestamp)
  "iss": "CarAuction",                // Emisor
  "aud": "CarAuctionClient"           // Audiencia
}
```

### Configuración JWT (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "CLAVE-SECRETA-MINIMO-32-CARACTERES-SEGURA",
    "Issuer": "CarAuction",
    "Audience": "CarAuctionClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Validación del Token

```csharp
// TokenService.cs:71-102
tokenHandler.ValidateToken(token, new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidateAudience = true,
    ValidAudience = jwtSettings["Audience"],
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero  // Sin tolerancia de tiempo
}, out var validatedToken);
```

---

## Refresh Tokens

### Modelo de Datos

```csharp
// RefreshToken.cs
public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public string Token { get; set; }           // 64 bytes aleatorios, Base64
    public DateTime ExpiresAt { get; set; }     // 7 días por defecto
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; } // Token rotado
    public string? IpAddress { get; set; }       // Tracking
    public string? UserAgent { get; set; }       // Tracking

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
```

### Rotación de Tokens

```
┌─────────────────────────────────────────────────────────────────┐
│                    REFRESH TOKEN ROTATION                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Token A (activo)                                               │
│      │                                                          │
│      │ POST /refresh-token                                      │
│      ▼                                                          │
│  ┌─────────────────┐                                            │
│  │ Validar Token A │                                            │
│  │ IsActive?       │                                            │
│  └────────┬────────┘                                            │
│           │                                                     │
│           ▼                                                     │
│  ┌─────────────────┐    ┌─────────────────┐                     │
│  │ Revocar Token A │───▶│ Token A         │                     │
│  │ IsRevoked=true  │    │ ReplacedByToken │                     │
│  │ RevokedAt=now   │    │ = Token B       │                     │
│  └─────────────────┘    └─────────────────┘                     │
│           │                                                     │
│           ▼                                                     │
│  ┌─────────────────┐                                            │
│  │ Crear Token B   │ ◀── Nuevo token activo                     │
│  │ (nuevo)         │                                            │
│  └─────────────────┘                                            │
│                                                                 │
│  Beneficio: Si Token A es robado y usado después de la         │
│  rotación, el servidor detecta que ya fue revocado.            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Hashing de Contraseñas

### BCrypt Configuration

```csharp
// AuthService.cs:45
PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12)

// AuthService.cs:105
BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
```

**Work Factor: 12**
- Produce ~250ms de tiempo de hash (balance seguridad/rendimiento)
- Resistente a ataques de fuerza bruta
- Auto-incluye salt único por contraseña

### Formato del Hash Almacenado

```
$2a$12$R4Jb8kQzN5mP7L2vX9Y1aeO3wF6hG5iJ4kL8mN0pQ2rS7tU9vW1xY
│  │  │                                                        │
│  │  └─ Salt (22 caracteres)                                  │
│  └─ Work factor (12)                                         │
└─ Versión algoritmo (2a)
```

---

## Sistema de Roles (RBAC)

### Roles Definidos

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| `User` | Usuario estándar | Ver subastas, pujar, ver perfil |
| `Admin` | Administrador | Todo + CRUD carros, gestión usuarios, dashboard |

### Autorización en Controladores

```csharp
// Requiere autenticación (cualquier rol)
[Authorize]
[HttpPost]
public async Task<IActionResult> PlaceBid(...) { }

// Requiere rol Admin
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<IActionResult> CreateCar(...) { }
```

### Controladores por Rol

| Controlador | Ruta | Autorización |
|-------------|------|--------------|
| `AuthController` | `/api/v1/auth/*` | Público (excepto logout) |
| `CarsController` | `/api/v1/cars/*` | Público (lectura) |
| `AdminCarsController` | `/api/v1/admin/cars/*` | Admin |
| `AuctionsController` | `/api/v1/auctions/*` | Público (lectura) |
| `AdminAuctionsController` | `/api/v1/admin/auctions/*` | Admin |
| `BidsController` | `/api/v1/bids/*` | Autenticado |
| `UsersController` | `/api/v1/users/*` | Autenticado |
| `AdminUsersController` | `/api/v1/admin/users/*` | Admin |
| `AdminController` | `/api/v1/admin/*` | Admin |

---

## Verificación de Email

### Flujo

```
1. Registro → Genera token (32 bytes, Base64 URL-safe)
2. Token almacenado en User.EmailVerificationToken
3. Expiración: 24 horas
4. Email enviado con link: /verify-email/{token}
5. Usuario hace clic → Token validado → EmailVerified = true
```

### Generación de Token Seguro

```csharp
// AuthService.cs:246-251
private static string GenerateToken()
{
    return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .Replace("+", "-")   // URL-safe
        .Replace("/", "_")   // URL-safe
        .Replace("=", "");   // Sin padding
}
```

---

## Recuperación de Contraseña

### Flujo Seguro

```
┌─────────────────────────────────────────────────────────────────┐
│                 PASSWORD RESET FLOW                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. POST /forgot-password { "email": "user@example.com" }      │
│                                                                 │
│  2. Servidor (AuthService.cs:187-203):                         │
│     ├─ Busca usuario por email                                 │
│     ├─ Si NO existe: return (no revelar)                       │
│     ├─ Si existe:                                               │
│     │   ├─ Genera PasswordResetToken                           │
│     │   ├─ Expiry = 1 hora                                     │
│     │   └─ Envía email con link                                │
│     │                                                           │
│  3. Respuesta SIEMPRE igual (seguridad):                       │
│     "Si el email existe, recibirás instrucciones..."           │
│                                                                 │
│  4. POST /reset-password                                        │
│     { "token": "...", "newPassword": "..." }                   │
│                                                                 │
│  5. Servidor (AuthService.cs:205-231):                         │
│     ├─ Valida token existe y no expiró                         │
│     ├─ Hash nueva contraseña (BCrypt 12)                       │
│     ├─ Limpia tokens de reset                                  │
│     └─ REVOCA TODOS los refresh tokens del usuario            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Seguridad clave**: Al resetear contraseña, se revocan TODOS los refresh tokens activos del usuario, cerrando todas las sesiones existentes.

---

## Rate Limiting (Pendiente de Configurar)

El paquete `AspNetCoreRateLimit` está instalado pero no configurado.

### Configuración Recomendada

```json
// appsettings.json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/v1/auth/login",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "POST:/api/v1/auth/register",
        "Period": "1h",
        "Limit": 3
      },
      {
        "Endpoint": "POST:/api/v1/auth/forgot-password",
        "Period": "1h",
        "Limit": 3
      }
    ]
  }
}
```

---

## Middleware de Excepciones

### Mapeo Excepciones → HTTP Status

```csharp
// ExceptionMiddleware.cs
private static int GetStatusCode(Exception exception) => exception switch
{
    NotFoundException => 404,
    BadRequestException => 400,
    UnauthorizedException => 401,
    ForbiddenException => 403,
    ConflictException => 409,
    ValidationException => 422,
    _ => 500
};
```

### Respuesta de Error Estandarizada

```json
{
  "success": false,
  "message": "Credenciales inválidas",
  "errors": null
}
```

---

## SignalR Authentication

### Autenticación via Query String

```csharp
// Program.cs - Configuración JWT para SignalR
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;

        if (!string.IsNullOrEmpty(accessToken) &&
            path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

### Conexión desde Cliente

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/auction?access_token=" + accessToken)
    .build();
```

---

## Recomendaciones de Seguridad

### Implementadas ✅

1. **Hashing seguro**: BCrypt con work factor 12
2. **Tokens rotativos**: Refresh tokens con revocación
3. **Claims mínimos**: Solo información necesaria en JWT
4. **Validación estricta**: ClockSkew = Zero
5. **No revelación de usuarios**: Forgot password respuesta genérica
6. **Revocación masiva**: Al cambiar contraseña, cierra todas las sesiones
7. **Tracking de sesiones**: IP y UserAgent registrados

### Pendientes de Implementar ⚠️

1. **Rate limiting**: Configurar AspNetCoreRateLimit
2. **Bloqueo de cuenta**: Tras N intentos fallidos
3. **2FA**: Autenticación de dos factores
4. **Auditoría**: Log de eventos de seguridad
5. **HTTPS forzado**: Redirect HTTP → HTTPS en producción
6. **Headers de seguridad**: HSTS, CSP, X-Frame-Options
7. **Refresh token en HttpOnly cookie**: Más seguro que localStorage

---

## Endpoints de Autenticación

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/auth/register` | Registro de usuario | No |
| POST | `/api/v1/auth/login` | Inicio de sesión | No |
| POST | `/api/v1/auth/refresh-token` | Renovar tokens | No |
| GET | `/api/v1/auth/verify-email/{token}` | Verificar email | No |
| POST | `/api/v1/auth/forgot-password` | Solicitar reset | No |
| POST | `/api/v1/auth/reset-password` | Cambiar contraseña | No |
| POST | `/api/v1/auth/logout` | Cerrar sesión | Sí |

---

## Archivos Relacionados

| Archivo | Ubicación | Responsabilidad |
|---------|-----------|-----------------|
| `AuthService.cs` | Infrastructure/Services | Lógica de autenticación |
| `TokenService.cs` | Infrastructure/Services | Generación/validación JWT |
| `AuthController.cs` | API/Controllers | Endpoints HTTP |
| `User.cs` | Domain/Entities | Entidad usuario |
| `RefreshToken.cs` | Domain/Entities | Entidad refresh token |
| `Role.cs` | Domain/Entities | Entidad rol |
| `UserRole.cs` | Domain/Entities | Relación usuario-rol |
| `ExceptionMiddleware.cs` | API/Middleware | Manejo de errores |
