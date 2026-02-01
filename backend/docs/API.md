# API REST

Documentación completa de la API RESTful del sistema de subastas CarAuction.

---

## Información General

| Atributo | Valor |
|----------|-------|
| Base URL | `https://localhost:7088/api/v1` |
| Formato | JSON |
| Autenticación | Bearer Token (JWT) |
| Versionado | URL path (`/api/v1/`) |
| Documentación | Swagger UI: `/swagger` |

---

## Convenciones

### Nomenclatura de Endpoints

```
GET    /resources          → Listar recursos (paginado)
GET    /resources/{id}     → Obtener recurso específico
POST   /resources          → Crear recurso
PUT    /resources/{id}     → Actualizar recurso completo
PATCH  /resources/{id}     → Actualizar parcialmente (no implementado)
DELETE /resources/{id}     → Eliminar recurso
POST   /resources/{id}/action → Acción específica
```

### Respuesta Estándar

Todas las respuestas usan el wrapper `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": { },
  "errors": null
}
```

**Respuesta de Error:**
```json
{
  "success": false,
  "message": "Descripción del error",
  "data": null,
  "errors": {
    "campo": ["Error de validación"]
  }
}
```

### Paginación

Endpoints que retornan listas usan `PaginatedResult<T>`:

```json
{
  "success": true,
  "data": {
    "items": [ ],
    "totalItems": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**Query Parameters:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | int | 1 | Número de página |
| `pageSize` | int | 10 | Elementos por página |

---

## Códigos de Estado HTTP

| Código | Significado | Uso |
|--------|-------------|-----|
| 200 | OK | Operación exitosa |
| 201 | Created | Recurso creado |
| 400 | Bad Request | Error de validación |
| 401 | Unauthorized | Token inválido/expirado |
| 403 | Forbidden | Sin permisos |
| 404 | Not Found | Recurso no existe |
| 409 | Conflict | Conflicto (ej: email duplicado) |
| 422 | Unprocessable Entity | Validación de negocio |
| 500 | Internal Server Error | Error del servidor |

---

## Autenticación

### Headers Requeridos

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json
```

### Niveles de Acceso

| Símbolo | Descripción |
|---------|-------------|
| 🔓 | Público (sin autenticación) |
| 🔐 | Requiere autenticación |
| 👑 | Requiere rol Admin |

---

## Endpoints

### Auth (Autenticación)

#### 🔓 POST /api/v1/auth/register
Registrar nuevo usuario.

**Request:**
```json
{
  "email": "usuario@example.com",
  "password": "Password123!",
  "firstName": "Juan",
  "lastName": "Pérez",
  "phoneNumber": "+52 555 123 4567"
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Registro exitoso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "abc123def456...",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "id": 1,
      "email": "usuario@example.com",
      "firstName": "Juan",
      "lastName": "Pérez",
      "fullName": "Juan Pérez",
      "roles": ["User"]
    }
  }
}
```

---

#### 🔓 POST /api/v1/auth/login
Iniciar sesión.

**Request:**
```json
{
  "email": "usuario@example.com",
  "password": "Password123!"
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Inicio de sesión exitoso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "abc123def456...",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "id": 1,
      "email": "usuario@example.com",
      "firstName": "Juan",
      "lastName": "Pérez",
      "fullName": "Juan Pérez",
      "roles": ["User"]
    }
  }
}
```

**Response 401:**
```json
{
  "success": false,
  "message": "Credenciales inválidas"
}
```

---

#### 🔓 POST /api/v1/auth/refresh-token
Renovar tokens.

**Request:**
```json
{
  "refreshToken": "abc123def456..."
}
```

**Response 200:** Misma estructura que login.

---

#### 🔓 GET /api/v1/auth/verify-email/{token}
Verificar email con token.

**Response 200:**
```json
{
  "success": true,
  "message": "Email verificado exitosamente"
}
```

---

#### 🔓 POST /api/v1/auth/forgot-password
Solicitar reset de contraseña.

**Request:**
```json
{
  "email": "usuario@example.com"
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Si el email existe, recibirás instrucciones para restablecer tu contraseña"
}
```

---

#### 🔓 POST /api/v1/auth/reset-password
Restablecer contraseña.

**Request:**
```json
{
  "token": "reset-token-here",
  "newPassword": "NewPassword123!"
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Contraseña restablecida exitosamente"
}
```

---

#### 🔐 POST /api/v1/auth/logout
Cerrar sesión (revocar refresh token).

**Request:**
```json
{
  "refreshToken": "abc123def456..."
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Sesión cerrada exitosamente"
}
```

---

### Cars (Vehículos)

#### 🔓 GET /api/v1/cars
Listar vehículos con filtros.

**Query Parameters:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `page` | int | Página |
| `pageSize` | int | Elementos por página |
| `brand` | string | Filtrar por marca |
| `minYear` | int | Año mínimo |
| `maxYear` | int | Año máximo |
| `isActive` | bool | Solo activos |

**Response 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "brand": "Toyota",
        "model": "Corolla",
        "year": 2022,
        "vin": "1HGBH41JXMN109186",
        "mileage": 25000,
        "color": "Blanco",
        "engineType": "I4",
        "transmission": "Automático",
        "fuelType": "Gasolina",
        "horsepower": 169,
        "description": "Excelente estado...",
        "condition": "Excelente",
        "features": ["Aire acondicionado", "Bluetooth", "Cámara trasera"],
        "isActive": true,
        "images": [
          {
            "id": 1,
            "imageUrl": "https://...",
            "thumbnailUrl": "https://...",
            "isPrimary": true,
            "displayOrder": 0
          }
        ],
        "createdAt": "2024-01-01T10:00:00Z"
      }
    ],
    "totalItems": 50,
    "page": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

---

#### 🔓 GET /api/v1/cars/{id}
Obtener vehículo por ID.

**Response 200:** Objeto `CarDto` completo.

**Response 404:**
```json
{
  "success": false,
  "message": "Carro no encontrado"
}
```

---

#### 🔓 GET /api/v1/cars/brands
Listar marcas disponibles.

**Response 200:**
```json
{
  "success": true,
  "data": ["Toyota", "Honda", "Ford", "Chevrolet", "BMW"]
}
```

---

#### 👑 POST /api/v1/admin/cars
Crear vehículo.

**Request:**
```json
{
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "vin": "1HGBH41JXMN109186",
  "mileage": 25000,
  "color": "Blanco",
  "engineType": "I4",
  "transmission": "Automático",
  "fuelType": "Gasolina",
  "horsepower": 169,
  "description": "Vehículo en excelente estado...",
  "condition": "Excelente",
  "features": ["Aire acondicionado", "Bluetooth"]
}
```

**Response 201:** Objeto `CarDto` creado.

---

#### 👑 PUT /api/v1/admin/cars/{id}
Actualizar vehículo.

**Request:** Misma estructura que POST.

**Response 200:** Objeto `CarDto` actualizado.

---

#### 👑 DELETE /api/v1/admin/cars/{id}
Eliminar vehículo.

**Response 200:**
```json
{
  "success": true,
  "message": "Carro eliminado exitosamente"
}
```

---

#### 👑 POST /api/v1/admin/cars/{id}/images
Agregar imagen a vehículo.

**Request:**
```json
{
  "imageUrl": "https://storage.example.com/car1.jpg",
  "thumbnailUrl": "https://storage.example.com/car1_thumb.jpg",
  "isPrimary": true
}
```

**Response 200:** Objeto `CarImageDto`.

---

#### 👑 DELETE /api/v1/admin/cars/{carId}/images/{imageId}
Eliminar imagen.

**Response 200:**
```json
{
  "success": true,
  "message": "Imagen eliminada exitosamente"
}
```

---

#### 👑 PUT /api/v1/admin/cars/{carId}/images/{imageId}/primary
Establecer imagen principal.

**Response 200:**
```json
{
  "success": true,
  "message": "Imagen principal actualizada"
}
```

---

### Auctions (Subastas)

#### 🔓 GET /api/v1/auctions
Listar todas las subastas.

**Query Parameters:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `page` | int | Página |
| `pageSize` | int | Elementos por página |
| `status` | string | Filtrar por estado |
| `brand` | string | Filtrar por marca del carro |

**Response 200:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "currentBid": 15000.00,
        "endTime": "2024-01-15T18:00:00Z",
        "totalBids": 12,
        "status": "Active",
        "carBrand": "Toyota",
        "carModel": "Corolla",
        "carYear": 2022,
        "primaryImage": "https://...",
        "remainingSeconds": 3600
      }
    ],
    "totalItems": 25,
    "page": 1,
    "pageSize": 10
  }
}
```

---

#### 🔓 GET /api/v1/auctions/active
Listar solo subastas activas.

**Response 200:** Misma estructura que GET /auctions.

---

#### 🔓 GET /api/v1/auctions/{id}
Obtener subasta con detalle completo.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "carId": 1,
    "startingPrice": 10000.00,
    "reservePrice": 15000.00,
    "minimumBidIncrement": 100.00,
    "currentBid": 15500.00,
    "currentBidderId": 5,
    "currentBidderName": "Juan P.",
    "startTime": "2024-01-01T10:00:00Z",
    "endTime": "2024-01-15T18:00:00Z",
    "totalBids": 15,
    "status": "Active",
    "remainingSeconds": 3600,
    "car": {
      "id": 1,
      "brand": "Toyota",
      "model": "Corolla",
      "year": 2022,
      "images": [...]
    },
    "createdAt": "2024-01-01T09:00:00Z"
  }
}
```

---

#### 🔓 GET /api/v1/auctions/{id}/bids
Obtener historial de pujas de una subasta.

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": 50,
      "auctionId": 1,
      "userId": 5,
      "userName": "Juan P.",
      "amount": 15500.00,
      "isWinningBid": true,
      "createdAt": "2024-01-10T14:30:00Z"
    },
    {
      "id": 49,
      "auctionId": 1,
      "userId": 3,
      "userName": "María G.",
      "amount": 15400.00,
      "isWinningBid": false,
      "createdAt": "2024-01-10T14:25:00Z"
    }
  ]
}
```

---

#### 👑 POST /api/v1/admin/auctions
Crear subasta.

**Request:**
```json
{
  "carId": 1,
  "startingPrice": 10000.00,
  "reservePrice": 15000.00,
  "minimumBidIncrement": 100.00,
  "startTime": "2024-01-01T10:00:00Z",
  "endTime": "2024-01-15T18:00:00Z",
  "extensionMinutes": 5,
  "extensionThresholdMinutes": 2
}
```

**Response 201:** Objeto `AuctionDto` creado.

---

#### 👑 PUT /api/v1/admin/auctions/{id}
Actualizar subasta (solo si está en Pending).

**Request:** Misma estructura que POST.

**Response 200:** Objeto `AuctionDto` actualizado.

---

#### 👑 POST /api/v1/admin/auctions/{id}/cancel
Cancelar subasta.

**Response 200:**
```json
{
  "success": true,
  "message": "Subasta cancelada exitosamente"
}
```

---

### Bids (Pujas)

#### 🔐 POST /api/v1/bids
Realizar una puja.

**Request:**
```json
{
  "auctionId": 1,
  "amount": 15600.00
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Puja realizada exitosamente",
  "data": {
    "bidId": 51,
    "newCurrentBid": 15600.00,
    "totalBids": 16,
    "newEndTime": "2024-01-15T18:05:00Z",
    "timeExtended": true
  }
}
```

**Response 400 (validación):**
```json
{
  "success": false,
  "message": "La puja debe ser mayor a la puja actual más el incremento mínimo"
}
```

**Evento SignalR emitido:** `BidPlaced`
```json
{
  "auctionId": 1,
  "currentBid": 15600.00,
  "totalBids": 16,
  "newEndTime": "2024-01-15T18:05:00Z",
  "timeExtended": true
}
```

---

### Users (Usuarios)

#### 🔐 GET /api/v1/users/me
Obtener perfil del usuario actual.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "email": "usuario@example.com",
    "firstName": "Juan",
    "lastName": "Pérez",
    "phoneNumber": "+52 555 123 4567",
    "status": "Active",
    "emailVerified": true,
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

---

#### 🔐 PUT /api/v1/users/me
Actualizar perfil.

**Request:**
```json
{
  "firstName": "Juan Carlos",
  "lastName": "Pérez García",
  "phoneNumber": "+52 555 987 6543"
}
```

**Response 200:** Objeto `UserDto` actualizado.

---

#### 🔐 POST /api/v1/users/me/change-password
Cambiar contraseña.

**Request:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!"
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Contraseña actualizada exitosamente"
}
```

---

#### 🔐 GET /api/v1/users/me/bids
Obtener historial de pujas del usuario.

**Query Parameters:** `page`, `pageSize`

**Response 200:** `PaginatedResult<BidDto>`

---

#### 👑 GET /api/v1/admin/users
Listar todos los usuarios.

**Response 200:** `PaginatedResult<UserDto>`

---

#### 👑 GET /api/v1/admin/users/{id}
Obtener usuario por ID.

**Response 200:** Objeto `UserDto`.

---

#### 👑 POST /api/v1/admin/users/{id}/activate
Activar usuario.

**Response 200:**
```json
{
  "success": true,
  "message": "Usuario activado exitosamente"
}
```

---

#### 👑 POST /api/v1/admin/users/{id}/deactivate
Desactivar usuario.

**Response 200:**
```json
{
  "success": true,
  "message": "Usuario desactivado exitosamente"
}
```

---

### Admin (Dashboard)

#### 👑 GET /api/v1/admin/dashboard
Obtener estadísticas del dashboard.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "totalUsers": 150,
    "activeUsers": 120,
    "totalCars": 45,
    "totalAuctions": 30,
    "activeAuctions": 10,
    "completedAuctions": 18,
    "totalBids": 450,
    "totalRevenue": 750000.00,
    "recentAuctions": [...],
    "topBidders": [...]
  }
}
```

---

## SignalR (Tiempo Real)

### Conexión

**URL:** `wss://localhost:7088/hubs/auction?access_token={JWT}`

### Métodos del Cliente → Servidor

| Método | Parámetros | Descripción |
|--------|------------|-------------|
| `JoinAuction` | `auctionId: int` | Suscribirse a actualizaciones |
| `LeaveAuction` | `auctionId: int` | Desuscribirse |

### Eventos del Servidor → Cliente

| Evento | Payload | Descripción |
|--------|---------|-------------|
| `BidPlaced` | `{ auctionId, currentBid, totalBids, newEndTime, timeExtended }` | Nueva puja |
| `AuctionsClosed` | `{ closedAuctionIds: int[] }` | Subastas cerradas |

### Ejemplo JavaScript

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/hubs/auction?access_token=${accessToken}`)
    .withAutomaticReconnect()
    .build();

// Escuchar eventos
connection.on("BidPlaced", (data) => {
    console.log(`Nueva puja: $${data.currentBid}`);
    if (data.timeExtended) {
        console.log(`Tiempo extendido hasta: ${data.newEndTime}`);
    }
});

connection.on("AuctionsClosed", (data) => {
    console.log(`Subastas cerradas: ${data.closedAuctionIds}`);
});

// Conectar
await connection.start();

// Suscribirse a una subasta
await connection.invoke("JoinAuction", 1);

// Desuscribirse
await connection.invoke("LeaveAuction", 1);
```

---

## Resumen de Endpoints

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| POST | `/auth/register` | 🔓 | Registro |
| POST | `/auth/login` | 🔓 | Login |
| POST | `/auth/refresh-token` | 🔓 | Refresh |
| GET | `/auth/verify-email/{token}` | 🔓 | Verificar email |
| POST | `/auth/forgot-password` | 🔓 | Forgot password |
| POST | `/auth/reset-password` | 🔓 | Reset password |
| POST | `/auth/logout` | 🔐 | Logout |
| GET | `/cars` | 🔓 | Listar carros |
| GET | `/cars/{id}` | 🔓 | Obtener carro |
| GET | `/cars/brands` | 🔓 | Listar marcas |
| POST | `/admin/cars` | 👑 | Crear carro |
| PUT | `/admin/cars/{id}` | 👑 | Actualizar carro |
| DELETE | `/admin/cars/{id}` | 👑 | Eliminar carro |
| POST | `/admin/cars/{id}/images` | 👑 | Agregar imagen |
| DELETE | `/admin/cars/{carId}/images/{imageId}` | 👑 | Eliminar imagen |
| PUT | `/admin/cars/{carId}/images/{imageId}/primary` | 👑 | Set imagen principal |
| GET | `/auctions` | 🔓 | Listar subastas |
| GET | `/auctions/active` | 🔓 | Subastas activas |
| GET | `/auctions/{id}` | 🔓 | Obtener subasta |
| GET | `/auctions/{id}/bids` | 🔓 | Historial de pujas |
| POST | `/admin/auctions` | 👑 | Crear subasta |
| PUT | `/admin/auctions/{id}` | 👑 | Actualizar subasta |
| POST | `/admin/auctions/{id}/cancel` | 👑 | Cancelar subasta |
| POST | `/bids` | 🔐 | Realizar puja |
| GET | `/users/me` | 🔐 | Mi perfil |
| PUT | `/users/me` | 🔐 | Actualizar perfil |
| POST | `/users/me/change-password` | 🔐 | Cambiar contraseña |
| GET | `/users/me/bids` | 🔐 | Mis pujas |
| GET | `/admin/users` | 👑 | Listar usuarios |
| GET | `/admin/users/{id}` | 👑 | Obtener usuario |
| POST | `/admin/users/{id}/activate` | 👑 | Activar usuario |
| POST | `/admin/users/{id}/deactivate` | 👑 | Desactivar usuario |
| GET | `/admin/dashboard` | 👑 | Dashboard stats |

---

## Archivos Relacionados

| Archivo | Ubicación |
|---------|-----------|
| AuthController.cs | API/Controllers/ |
| CarsController.cs | API/Controllers/ |
| AuctionsController.cs | API/Controllers/ |
| BidsController.cs | API/Controllers/ |
| UsersController.cs | API/Controllers/ |
| AdminController.cs | API/Controllers/ |
| AuctionHub.cs | API/Hubs/ |
| DTOs/*.cs | Application/DTOs/ |
