# Base de Datos

Documentación del modelo de datos, configuraciones de Entity Framework Core y estrategia de migraciones.

---

## Tecnología

| Componente | Valor |
|------------|-------|
| SGBD | MySQL 8.0+ |
| ORM | Entity Framework Core 8.0 |
| Provider | Pomelo.EntityFrameworkCore.MySql |
| Patrón | Code-First con Fluent API |

---

## Diagrama Entidad-Relación

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                    MODELO RELACIONAL                                    │
└─────────────────────────────────────────────────────────────────────────────────────────┘

┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│    Users     │────┬───▶│  UserRoles   │◀───────▶│    Roles     │
│──────────────│    │    │──────────────│         │──────────────│
│ Id (PK)      │    │    │ UserId (FK)  │         │ Id (PK)      │
│ Email (UQ)   │    │    │ RoleId (FK)  │         │ Name         │
│ PasswordHash │    │    └──────────────┘         │ Description  │
│ FirstName    │    │                             └──────────────┘
│ LastName     │    │
│ PhoneNumber  │    │    ┌──────────────┐         ┌──────────────┐
│ Status       │    ├───▶│RefreshTokens │         │    Cars      │
│ EmailVerified│    │    │──────────────│         │──────────────│
│ ...tokens    │    │    │ Id (PK)      │         │ Id (PK)      │
│ LastLoginAt  │    │    │ UserId (FK)  │         │ Brand        │
│ CreatedAt    │    │    │ Token        │         │ Model        │
│ UpdatedAt    │    │    │ ExpiresAt    │         │ Year         │
└──────┬───────┘    │    │ IsRevoked    │         │ VIN (UQ)     │
       │            │    │ ...          │         │ Mileage      │
       │            │    └──────────────┘         │ Color        │
       │            │                             │ EngineType   │
       │            │    ┌──────────────┐         │ Transmission │
       │            ├───▶│Notifications │         │ FuelType     │
       │            │    │──────────────│         │ Horsepower   │
       │            │    │ Id (PK)      │         │ Description  │
       │            │    │ UserId (FK)  │         │ Condition    │
       │            │    │ AuctionId(FK)│────┐    │ Features JSON│
       │            │    │ Type         │    │    │ IsActive     │
       │            │    │ Title        │    │    │ CreatedAt    │
       │            │    │ Message      │    │    │ UpdatedAt    │
       │            │    │ IsRead       │    │    └──────┬───────┘
       │            │    └──────────────┘    │           │
       │            │                        │           │ 1:1
       │            │                        │           ▼
       │            │                        │    ┌──────────────┐
       │            │    ┌──────────────┐    │    │   Auctions   │
       │            └───▶│    Bids      │    │    │──────────────│
       │                 │──────────────│    │    │ Id (PK)      │
       │                 │ Id (PK)      │    │    │ CarId (FK)   │◀── 1:1
       │                 │ AuctionId(FK)│────┼───▶│ StartingPrice│
       │                 │ UserId (FK)  │    │    │ ReservePrice │
       │                 │ Amount       │    │    │ MinBidIncr   │
       │                 │ IsWinningBid │    │    │ CurrentBid   │
       │                 │ IpAddress    │    │    │CurrentBidderId│───┐
       │                 │ CreatedAt    │    │    │ StartTime    │   │
       │                 └──────────────┘    │    │ EndTime      │   │
       │                                     │    │ OriginalEnd  │   │
       │                                     │    │ ExtensionMin │   │
       │                 ┌──────────────┐    │    │ ExtThreshold │   │
       └─────────────────│AuctionHistory│    │    │ TotalBids    │   │
                         │──────────────│    │    │ Status       │   │
                         │ Id (PK)      │    │    │ CreatedAt    │   │
                         │ AuctionId(FK)│◀───┘    │ UpdatedAt    │   │
                         │ WinnerId(FK) │◀────────└──────────────┘   │
                         │ FinalPrice   │                            │
                         │ TotalBids    │    ┌──────────────┐        │
                         │ UniqueParts  │    │  CarImages   │        │
                         │ CompletedAt  │    │──────────────│        │
                         │ ReserveMet   │    │ Id (PK)      │        │
                         │ Notes        │    │ CarId (FK)   │◀───────┘
                         └──────────────┘    │ ImageUrl     │
                                             │ ThumbnailUrl │
                                             │ IsPrimary    │
                                             │ DisplayOrder │
                                             └──────────────┘
```

---

## Entidades del Dominio

### BaseEntity (Clase Base)

```csharp
// Domain/Common/BaseEntity.cs
public abstract class BaseEntity
{
    public int Id { get; set; }                          // PK auto-increment
    public DateTime CreatedAt { get; set; }              // Auto-set en INSERT
    public DateTime? UpdatedAt { get; set; }             // Auto-set en UPDATE
}
```

**Auditoría automática** implementada en `ApplicationDbContext.SaveChangesAsync()`:
- `CreatedAt`: Se establece automáticamente al crear la entidad
- `UpdatedAt`: Se actualiza automáticamente al modificar la entidad

---

### Users (Usuarios)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador único |
| Email | VARCHAR(255) | UNIQUE, NOT NULL | Correo electrónico |
| PasswordHash | VARCHAR(255) | NOT NULL | Hash BCrypt |
| FirstName | VARCHAR(100) | NOT NULL | Nombre |
| LastName | VARCHAR(100) | NOT NULL | Apellido |
| PhoneNumber | VARCHAR(20) | NULL | Teléfono |
| Status | TINYINT | NOT NULL | 0=Pending, 1=Active |
| EmailVerified | BIT | NOT NULL, DEFAULT 0 | Email confirmado |
| EmailVerificationToken | VARCHAR(255) | NULL | Token de verificación |
| EmailVerificationTokenExpiry | DATETIME | NULL | Expiración del token |
| PasswordResetToken | VARCHAR(255) | NULL | Token reset password |
| PasswordResetTokenExpiry | DATETIME | NULL | Expiración reset |
| LastLoginAt | DATETIME | NULL | Último acceso |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Relaciones:**
- `1:N` → UserRoles (roles del usuario)
- `1:N` → Bids (pujas realizadas)
- `1:N` → RefreshTokens (sesiones activas)
- `1:N` → Notifications (notificaciones)

---

### Roles

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| Name | VARCHAR(50) | UNIQUE, NOT NULL | Nombre del rol |
| Description | VARCHAR(255) | NULL | Descripción |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Valores esperados:**
- `User` - Usuario estándar
- `Admin` - Administrador

---

### UserRoles (Relación N:M)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| UserId | INT | PK, FK → Users | Usuario |
| RoleId | INT | PK, FK → Roles | Rol |

**Clave compuesta:** (UserId, RoleId)

---

### Cars (Vehículos)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| Brand | VARCHAR(100) | NOT NULL | Marca (Toyota, Honda) |
| Model | VARCHAR(100) | NOT NULL | Modelo (Corolla, Civic) |
| Year | INT | NOT NULL | Año de fabricación |
| VIN | VARCHAR(17) | UNIQUE, NULL | Número de identificación |
| Mileage | INT | NOT NULL | Kilometraje |
| Color | VARCHAR(50) | NULL | Color exterior |
| EngineType | VARCHAR(50) | NULL | Tipo motor (V6, I4) |
| Transmission | VARCHAR(50) | NULL | Manual/Automático |
| FuelType | VARCHAR(50) | NULL | Gasolina/Diesel/Eléctrico |
| Horsepower | INT | NULL | Caballos de fuerza |
| Description | TEXT | NULL | Descripción detallada |
| Condition | VARCHAR(50) | NULL | Excelente/Bueno/Regular |
| Features | JSON | NULL | Lista de características |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Activo en sistema |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Formato Features (JSON):**
```json
["Aire acondicionado", "Bluetooth", "Cámara trasera", "Asientos de cuero"]
```

**Relaciones:**
- `1:N` → CarImages (imágenes)
- `1:1` → Auction (subasta activa)

---

### CarImages (Imágenes)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| CarId | INT | FK → Cars, NOT NULL | Vehículo |
| ImageUrl | VARCHAR(500) | NOT NULL | URL imagen completa |
| ThumbnailUrl | VARCHAR(500) | NULL | URL thumbnail |
| IsPrimary | BIT | NOT NULL, DEFAULT 0 | Imagen principal |
| DisplayOrder | INT | NOT NULL, DEFAULT 0 | Orden de visualización |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Regla de negocio:** Solo una imagen por carro puede tener `IsPrimary = true`

---

### Auctions (Subastas)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| CarId | INT | FK → Cars, UNIQUE, NOT NULL | Vehículo (1:1) |
| StartingPrice | DECIMAL(18,2) | NOT NULL | Precio inicial |
| ReservePrice | DECIMAL(18,2) | NULL | Precio de reserva |
| MinimumBidIncrement | DECIMAL(18,2) | NOT NULL, DEFAULT 100 | Incremento mínimo |
| CurrentBid | DECIMAL(18,2) | NOT NULL | Puja actual |
| CurrentBidderId | INT | FK → Users, NULL | Pujador líder |
| StartTime | DATETIME | NOT NULL | Inicio de subasta |
| EndTime | DATETIME | NOT NULL | Fin de subasta |
| OriginalEndTime | DATETIME | NULL | Fin original (antes de extensiones) |
| ExtensionMinutes | INT | NOT NULL, DEFAULT 5 | Minutos a extender |
| ExtensionThresholdMinutes | INT | NOT NULL, DEFAULT 2 | Umbral para extensión |
| TotalBids | INT | NOT NULL, DEFAULT 0 | Contador de pujas |
| Status | TINYINT | NOT NULL | Estado (enum) |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Índices:**
```sql
CREATE INDEX IX_Auctions_Status ON Auctions(Status);
CREATE INDEX IX_Auctions_EndTime ON Auctions(EndTime);
CREATE INDEX IX_Auctions_Status_EndTime ON Auctions(Status, EndTime);
```

**Estados (AuctionStatus):**
| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Pending | Programada, no iniciada |
| 1 | Active | En curso |
| 2 | Completed | Finalizada |
| 3 | Cancelled | Cancelada |

---

### Bids (Pujas)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| AuctionId | INT | FK → Auctions, NOT NULL | Subasta |
| UserId | INT | FK → Users, NOT NULL | Usuario que puja |
| Amount | DECIMAL(18,2) | NOT NULL | Monto de la puja |
| IsWinningBid | BIT | NOT NULL, DEFAULT 0 | Es puja ganadora |
| IpAddress | VARCHAR(45) | NULL | IP del cliente |
| CreatedAt | DATETIME | NOT NULL | Fecha de la puja |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Índices sugeridos:**
```sql
CREATE INDEX IX_Bids_AuctionId ON Bids(AuctionId);
CREATE INDEX IX_Bids_UserId ON Bids(UserId);
CREATE INDEX IX_Bids_AuctionId_Amount ON Bids(AuctionId, Amount DESC);
```

---

### AuctionHistory (Historial)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| AuctionId | INT | FK → Auctions, UNIQUE | Subasta (1:1) |
| WinnerId | INT | FK → Users, NULL | Ganador |
| FinalPrice | DECIMAL(18,2) | NULL | Precio final |
| TotalBids | INT | NOT NULL | Total de pujas |
| UniqueParticipants | INT | NOT NULL | Participantes únicos |
| CompletedAt | DATETIME | NULL | Fecha de cierre |
| ReserveMet | BIT | NOT NULL | Reserva alcanzada |
| Notes | TEXT | NULL | Notas adicionales |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

---

### Notifications (Notificaciones)

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| UserId | INT | FK → Users, NOT NULL | Usuario destino |
| Type | TINYINT | NOT NULL | Tipo de notificación |
| Title | VARCHAR(200) | NOT NULL | Título |
| Message | TEXT | NOT NULL | Mensaje completo |
| AuctionId | INT | FK → Auctions, NULL | Subasta relacionada |
| IsRead | BIT | NOT NULL, DEFAULT 0 | Leída |
| ReadAt | DATETIME | NULL | Fecha de lectura |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

**Tipos (NotificationType):**
| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Outbid | Superado en puja |
| 1 | AuctionWon | Ganaste la subasta |
| 2 | AuctionEnding | Subasta por terminar |
| 3 | AuctionCancelled | Subasta cancelada |
| 4 | NewAuction | Nueva subasta disponible |
| 5 | BidPlaced | Puja registrada |

---

### RefreshTokens

| Columna | Tipo | Constraints | Descripción |
|---------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Identificador |
| UserId | INT | FK → Users, NOT NULL | Usuario |
| Token | VARCHAR(255) | NOT NULL | Token único |
| ExpiresAt | DATETIME | NOT NULL | Expiración |
| IsRevoked | BIT | NOT NULL, DEFAULT 0 | Revocado |
| RevokedAt | DATETIME | NULL | Fecha revocación |
| ReplacedByToken | VARCHAR(255) | NULL | Token sucesor |
| IpAddress | VARCHAR(45) | NULL | IP del cliente |
| UserAgent | TEXT | NULL | User-Agent |
| CreatedAt | DATETIME | NOT NULL | Fecha creación |
| UpdatedAt | DATETIME | NULL | Última modificación |

---

## Configuraciones Fluent API

### Comportamiento de Eliminación

| Relación | DeleteBehavior | Razón |
|----------|----------------|-------|
| Auction → Car | Restrict | No eliminar carro con subasta |
| Auction → CurrentBidder | SetNull | Permite eliminar usuario |
| Bid → Auction | Cascade | Eliminar pujas con subasta |
| Bid → User | Restrict | No eliminar usuario con pujas |
| CarImage → Car | Cascade | Eliminar imágenes con carro |
| UserRole → User | Cascade | Eliminar roles con usuario |
| RefreshToken → User | Cascade | Eliminar tokens con usuario |
| Notification → User | Cascade | Eliminar notificaciones con usuario |

### Precisión Decimal

Todos los campos monetarios usan `DECIMAL(18,2)`:
- StartingPrice, ReservePrice, CurrentBid
- MinimumBidIncrement, Amount, FinalPrice

---

## Migraciones EF Core

### Configuración Inicial

```bash
# Desde la raíz del proyecto
cd backend

# Instalar herramienta EF Core (si no está instalada)
dotnet tool install --global dotnet-ef

# Crear migración inicial
dotnet ef migrations add InitialCreate \
  -p src/CarAuction.Infrastructure \
  -s src/CarAuction.API

# Aplicar migración
dotnet ef database update \
  -p src/CarAuction.Infrastructure \
  -s src/CarAuction.API
```

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=carauction;User=root;Password=yourpassword;CharSet=utf8mb4;"
  }
}
```

### Comandos Útiles

```bash
# Listar migraciones
dotnet ef migrations list -p src/CarAuction.Infrastructure -s src/CarAuction.API

# Revertir última migración
dotnet ef migrations remove -p src/CarAuction.Infrastructure -s src/CarAuction.API

# Generar script SQL
dotnet ef migrations script -p src/CarAuction.Infrastructure -s src/CarAuction.API -o migration.sql

# Revertir a migración específica
dotnet ef database update MigrationName -p src/CarAuction.Infrastructure -s src/CarAuction.API
```

---

## Seed Data Recomendado

```csharp
// Agregar en una migración o configuración
modelBuilder.Entity<Role>().HasData(
    new Role { Id = 1, Name = "Admin", Description = "Administrador del sistema" },
    new Role { Id = 2, Name = "User", Description = "Usuario estándar" }
);

// Usuario admin inicial (cambiar contraseña en producción)
modelBuilder.Entity<User>().HasData(
    new User
    {
        Id = 1,
        Email = "admin@carauction.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12),
        FirstName = "Admin",
        LastName = "System",
        Status = UserStatus.Active,
        EmailVerified = true
    }
);

modelBuilder.Entity<UserRole>().HasData(
    new UserRole { UserId = 1, RoleId = 1 }
);
```

---

## Optimizaciones de Rendimiento

### Índices Implementados

```sql
-- Auctions (ya configurados en AuctionConfiguration.cs)
CREATE INDEX IX_Auctions_Status ON Auctions(Status);
CREATE INDEX IX_Auctions_EndTime ON Auctions(EndTime);
CREATE INDEX IX_Auctions_Status_EndTime ON Auctions(Status, EndTime);
```

### Índices Recomendados (Adicionales)

```sql
-- Búsqueda de usuarios por email (login)
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- Búsqueda de pujas por subasta
CREATE INDEX IX_Bids_AuctionId_CreatedAt ON Bids(AuctionId, CreatedAt DESC);

-- Notificaciones no leídas por usuario
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead)
WHERE IsRead = 0;

-- Refresh tokens activos
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token)
WHERE IsRevoked = 0;

-- Carros por marca (filtros)
CREATE INDEX IX_Cars_Brand ON Cars(Brand);
```

---

## Buenas Prácticas

### ✅ Implementadas

1. **Soft timestamps**: CreatedAt/UpdatedAt automáticos
2. **Fluent API**: Configuraciones separadas por entidad
3. **Índices compuestos**: Status + EndTime para consultas de subastas
4. **Precisión decimal**: 18,2 para valores monetarios
5. **Restricciones FK**: Prevent orphan records

### ⚠️ Recomendaciones Pendientes

1. **Soft delete**: Agregar campo `IsDeleted` en lugar de eliminar registros
2. **Auditoría completa**: Tabla AuditLog para cambios críticos
3. **Particionamiento**: Tabla Bids puede crecer rápido
4. **Read replicas**: Para consultas pesadas de reportes
5. **Índices fulltext**: Para búsqueda de descripciones de carros

---

## Archivos Relacionados

| Archivo | Ubicación |
|---------|-----------|
| ApplicationDbContext.cs | Infrastructure/Data/ |
| UserConfiguration.cs | Infrastructure/Data/Configurations/ |
| RoleConfiguration.cs | Infrastructure/Data/Configurations/ |
| CarConfiguration.cs | Infrastructure/Data/Configurations/ |
| AuctionConfiguration.cs | Infrastructure/Data/Configurations/ |
| BidConfiguration.cs | Infrastructure/Data/Configurations/ |
| (+ 5 más configuraciones) | Infrastructure/Data/Configurations/ |
