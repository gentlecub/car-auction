# CarAuction Backend

Sistema de subastas de vehículos en tiempo real construido con **ASP.NET Core 8.0**, arquitectura limpia y comunicación bidireccional mediante **SignalR**.

---

## Arquitectura del Sistema

### Patrón: Clean Architecture (Arquitectura Limpia)

```
┌─────────────────────────────────────────────────────────────────┐
│                        CarAuction.API                           │
│  (Controllers, Hubs, Middleware, BackgroundServices)            │
├─────────────────────────────────────────────────────────────────┤
│                   CarAuction.Infrastructure                     │
│  (DbContext, Services, Entity Configurations)                   │
├─────────────────────────────────────────────────────────────────┤
│                    CarAuction.Application                       │
│  (DTOs, Interfaces, Validators, Mappings)                       │
├─────────────────────────────────────────────────────────────────┤
│                      CarAuction.Domain                          │
│  (Entities, Enums, Exceptions, Base Classes)                    │
└─────────────────────────────────────────────────────────────────┘
```

### Principio de Dependencia

```
API → Infrastructure → Application → Domain
         ↓                  ↓            ↓
    (implementa)      (define)      (núcleo)
```

- **Domain**: Capa más interna. Sin dependencias externas.
- **Application**: Define contratos (interfaces) y DTOs.
- **Infrastructure**: Implementa contratos, acceso a datos.
- **API**: Punto de entrada HTTP/WebSocket.

---

## Stack Tecnológico

| Componente          | Tecnología                        | Versión |
|---------------------|-----------------------------------|---------|
| Framework           | ASP.NET Core                      | 8.0     |
| ORM                 | Entity Framework Core             | 8.0     |
| Base de Datos       | MySQL (Pomelo Provider)           | 8.0+    |
| Autenticación       | JWT Bearer Tokens                 | -       |
| Tiempo Real         | SignalR                           | 8.0     |
| Validación          | FluentValidation                  | 11.9.0  |
| Mapeo               | AutoMapper                        | 13.0.1  |
| Hash de Contraseñas | BCrypt.Net-Next                   | 4.0.3   |
| Documentación API   | Swagger/Swashbuckle               | 6.5.0   |
| Caché (pendiente)   | Redis (StackExchangeRedis)        | -       |
| Rate Limiting       | AspNetCoreRateLimit               | 5.0.0   |

---

## Estructura de Carpetas

```
/backend
├── CarAuction.sln                    # Solución principal
└── /src
    ├── CarAuction.Domain/
    │   ├── /Common
    │   │   └── BaseEntity.cs         # Clase base (Id, CreatedAt, UpdatedAt)
    │   ├── /Entities
    │   │   ├── User.cs               # Usuario del sistema
    │   │   ├── Role.cs               # Roles (Admin, User)
    │   │   ├── UserRole.cs           # Relación muchos-a-muchos
    │   │   ├── Car.cs                # Vehículo en subasta
    │   │   ├── CarImage.cs           # Imágenes del vehículo
    │   │   ├── Auction.cs            # Subasta activa
    │   │   ├── Bid.cs                # Puja individual
    │   │   ├── AuctionHistory.cs     # Historial de subastas cerradas
    │   │   ├── Notification.cs       # Notificaciones al usuario
    │   │   └── RefreshToken.cs       # Tokens de refresco JWT
    │   ├── /Enums
    │   │   ├── AuctionStatus.cs      # Pending, Active, Completed, Cancelled
    │   │   ├── UserStatus.cs         # Pending, Active
    │   │   └── NotificationType.cs   # Outbid, AuctionWon, etc.
    │   └── /Exceptions
    │       ├── DomainException.cs    # Excepción base
    │       ├── NotFoundException.cs
    │       ├── BadRequestException.cs
    │       ├── UnauthorizedException.cs
    │       ├── ForbiddenException.cs
    │       ├── ConflictException.cs
    │       └── ValidationException.cs
    │
    ├── CarAuction.Application/
    │   ├── /DTOs
    │   │   ├── /Auth                 # Login, Register, Token requests
    │   │   ├── /Car                  # Car CRUD DTOs
    │   │   ├── /Auction              # Auction DTOs y filtros
    │   │   ├── /Bid                  # Bid DTOs
    │   │   ├── /User                 # User profile DTOs
    │   │   └── /Common               # ApiResponse<T>, PaginatedResult<T>
    │   ├── /Interfaces
    │   │   ├── IAuthService.cs
    │   │   ├── ITokenService.cs
    │   │   ├── ICarService.cs
    │   │   ├── IAuctionService.cs
    │   │   ├── IBidService.cs
    │   │   ├── IUserService.cs
    │   │   ├── INotificationService.cs
    │   │   ├── IEmailService.cs
    │   │   └── IDashboardService.cs
    │   ├── /Validators               # FluentValidation rules
    │   ├── /Mappings
    │   │   └── MappingProfile.cs     # AutoMapper profiles
    │   └── DependencyInjection.cs
    │
    ├── CarAuction.Infrastructure/
    │   ├── /Data
    │   │   ├── ApplicationDbContext.cs
    │   │   └── /Configurations       # Fluent API configurations
    │   ├── /Services                 # Implementaciones de interfaces
    │   └── DependencyInjection.cs
    │
    └── CarAuction.API/
        ├── /Controllers
        │   ├── AuthController.cs
        │   ├── CarsController.cs
        │   ├── AdminCarsController.cs
        │   ├── AuctionsController.cs
        │   ├── AdminAuctionsController.cs
        │   ├── BidsController.cs
        │   ├── UsersController.cs
        │   ├── AdminUsersController.cs
        │   └── AdminController.cs
        ├── /Hubs
        │   └── AuctionHub.cs         # SignalR para tiempo real
        ├── /Middleware
        │   └── ExceptionMiddleware.cs
        ├── /BackgroundServices
        │   └── AuctionCloseService.cs
        ├── Program.cs
        ├── appsettings.json
        └── appsettings.Development.json
```

---

## Flujo de Datos

### 1. Request HTTP Estándar

```
Cliente HTTP
    │
    ▼
┌─────────────────┐
│  Middleware     │ ← ExceptionMiddleware (manejo global de errores)
│  Pipeline       │ ← JWT Authentication
│                 │ ← CORS
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Controller    │ ← Valida modelo (FluentValidation)
│                 │ ← Extrae claims del usuario
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Service      │ ← Lógica de negocio
│                 │ ← AutoMapper (Entity ↔ DTO)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   DbContext     │ ← Entity Framework Core
│                 │ ← MySQL
└─────────────────┘
```

### 2. Comunicación Tiempo Real (SignalR)

```
Cliente WebSocket
    │
    ▼
┌─────────────────┐
│   AuctionHub    │ ← Autenticación JWT via query string
│                 │ ← Grupos por subasta (JoinAuction/LeaveAuction)
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
BidPlaced  AuctionsClosed
(nueva puja) (subasta cerrada)
```

### 3. Procesamiento en Background

```
┌──────────────────────────┐
│  AuctionCloseService     │ ← Hosted Service
│  (cada 60 segundos)      │
└───────────┬──────────────┘
            │
            ▼
    ┌───────────────┐
    │ Verificar     │
    │ subastas      │
    │ expiradas     │
    └───────┬───────┘
            │
    ┌───────┴───────┐
    │               │
    ▼               ▼
Cerrar          Notificar
subasta         via SignalR
```

---

## Convenciones del Proyecto

### Nomenclatura

| Elemento        | Convención                  | Ejemplo                    |
|-----------------|-----------------------------|-----------------------------|
| Clases          | PascalCase                  | `AuctionService`           |
| Interfaces      | I + PascalCase              | `IAuctionService`          |
| Métodos         | PascalCase                  | `GetActiveAuctionsAsync`   |
| Variables       | camelCase                   | `currentBid`               |
| Constantes      | PascalCase                  | `MaxBidAmount`             |
| DTOs            | {Entity}Dto / {Action}Request | `CarDto`, `CreateBidRequest` |
| Controladores   | {Entity}Controller          | `AuctionsController`       |
| Rutas API       | kebab-case (plural)         | `/api/v1/auctions`         |

### Respuestas API Estandarizadas

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
```

### Paginación

```csharp
public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

---

## Escalabilidad

### Horizontal (Múltiples Instancias)

1. **SignalR con Redis Backplane** (pendiente de implementar)
   ```csharp
   builder.Services.AddSignalR()
       .AddStackExchangeRedis("redis-connection-string");
   ```

2. **Sesiones Stateless**: JWT permite escalar sin sesiones pegajosas.

3. **Base de Datos**: MySQL soporta réplicas de lectura.

### Vertical (Optimización)

1. **Caché con Redis**: Para consultas frecuentes (subastas activas, marcas de autos).
2. **Índices Optimizados**: `Status + EndTime` en tabla Auctions.
3. **Async/Await**: Todo el código es asíncrono.

---

## Despliegue

### Desarrollo Local

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar migraciones (pendiente de crear)
dotnet ef database update -p src/CarAuction.Infrastructure -s src/CarAuction.API

# Iniciar servidor
dotnet run --project src/CarAuction.API
```

**URLs locales**:
- HTTP: `http://localhost:5276`
- HTTPS: `https://localhost:7088`
- Swagger: `https://localhost:7088/swagger`

### Variables de Entorno Requeridas

| Variable              | Descripción                          |
|-----------------------|--------------------------------------|
| `ConnectionStrings__DefaultConnection` | Connection string MySQL     |
| `JwtSettings__SecretKey` | Clave secreta JWT (mín. 32 chars) |
| `JwtSettings__Issuer`    | Emisor del token                  |
| `JwtSettings__Audience`  | Audiencia del token               |
| `JwtSettings__AccessTokenExpirationMinutes` | Expiración access token |
| `JwtSettings__RefreshTokenExpirationDays`   | Expiración refresh token |

### Docker (ver docs/DEVOPS.md)

```bash
docker-compose up -d
```

---

## Estado Actual del Proyecto

### ✅ Implementado

- [x] Arquitectura Clean Architecture completa
- [x] Autenticación JWT con refresh tokens
- [x] CRUD completo de Carros, Subastas, Usuarios
- [x] Sistema de pujas con validación
- [x] SignalR para actualizaciones en tiempo real
- [x] Cierre automático de subastas (background service)
- [x] Middleware de excepciones centralizado
- [x] Validación con FluentValidation
- [x] Mapeo con AutoMapper
- [x] Roles y autorización (Admin/User)
- [x] Swagger/OpenAPI documentación

### ⚠️ Pendiente de Implementar

- [ ] Migraciones de Entity Framework
- [ ] Seed data (roles iniciales, admin)
- [ ] Tests unitarios e integración
- [ ] Caché con Redis
- [ ] Rate limiting configurado
- [ ] Health checks
- [ ] Logging estructurado (Serilog)
- [ ] Email service funcional (SMTP)
- [ ] Audit trail

---

## Documentación Adicional

| Documento | Descripción |
|-----------|-------------|
| [AUTH.md](docs/AUTH.md) | Autenticación, JWT, roles, seguridad |
| [DATABASE.md](docs/DATABASE.md) | Modelo de datos, migraciones |
| [API.md](docs/API.md) | Endpoints, ejemplos, versionado |
| [AUCTIONS.md](docs/AUCTIONS.md) | Lógica de subastas y pujas |
| [TESTING.md](docs/TESTING.md) | Estrategia de testing |
| [DEVOPS.md](docs/DEVOPS.md) | Docker, CI/CD, despliegue |

---

## Licencia

Proyecto privado. Todos los derechos reservados.
