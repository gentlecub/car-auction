# Sistema de Subastas

Documentación completa de la lógica de negocio del sistema de subastas en tiempo real.

---

## Resumen del Sistema

| Componente | Descripción |
|------------|-------------|
| Tipo | Subasta inglesa (precio ascendente) |
| Tiempo Real | SignalR WebSockets |
| Extensión | Anti-snipe (última hora) |
| Cierre | Automático (background service) |
| Concurrencia | Transacciones con rollback |

---

## Ciclo de Vida de una Subasta

```
┌─────────────────────────────────────────────────────────────────┐
│                    CICLO DE VIDA                                │
└─────────────────────────────────────────────────────────────────┘

    ┌──────────┐         ┌──────────┐         ┌───────────┐
    │ PENDING  │────────▶│  ACTIVE  │────────▶│ COMPLETED │
    └──────────┘         └──────────┘         └───────────┘
         │                    │
         │                    │
         │                    ▼
         │               ┌───────────┐
         └──────────────▶│ CANCELLED │
                         └───────────┘

Estados:
┌───────────┬────────────────────────────────────────────────────┐
│ PENDING   │ Subasta programada, StartTime > Now                │
│           │ - Se puede modificar libremente                    │
│           │ - No acepta pujas                                  │
├───────────┼────────────────────────────────────────────────────┤
│ ACTIVE    │ Subasta en curso, StartTime <= Now < EndTime       │
│           │ - Acepta pujas                                     │
│           │ - Solo modificable si TotalBids == 0               │
│           │ - Extensión automática anti-snipe                  │
├───────────┼────────────────────────────────────────────────────┤
│ COMPLETED │ Subasta finalizada, EndTime <= Now                 │
│           │ - No acepta más pujas                              │
│           │ - Se crea AuctionHistory                           │
│           │ - Se notifica al ganador (si aplica)               │
├───────────┼────────────────────────────────────────────────────┤
│ CANCELLED │ Subasta cancelada por admin                        │
│           │ - No acepta pujas                                  │
│           │ - Se notifica a todos los participantes            │
└───────────┴────────────────────────────────────────────────────┘
```

---

## Flujo de Creación de Subasta

```
┌─────────────────────────────────────────────────────────────────┐
│                 CREAR SUBASTA (Admin)                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Admin envía CreateAuctionRequest                           │
│     {                                                           │
│       "carId": 1,                                               │
│       "startingPrice": 10000,                                   │
│       "reservePrice": 15000,        // Opcional                 │
│       "minimumBidIncrement": 100,                               │
│       "startTime": "2024-01-15T10:00:00Z",                     │
│       "endTime": "2024-01-20T18:00:00Z",                       │
│       "extensionMinutes": 5,                                    │
│       "extensionThresholdMinutes": 2                            │
│     }                                                           │
│                                                                 │
│  2. Validaciones:                                               │
│     ├─ ¿Carro existe?                                          │
│     ├─ ¿Carro tiene subasta activa? → Error                    │
│     ├─ startingPrice > 0?                                       │
│     └─ endTime > startTime?                                     │
│                                                                 │
│  3. Creación:                                                   │
│     ├─ CurrentBid = StartingPrice                              │
│     ├─ OriginalEndTime = EndTime                               │
│     └─ Status = (StartTime <= Now) ? Active : Pending          │
│                                                                 │
│  4. Respuesta: AuctionDto completo                             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Ubicación del código**: `AuctionService.cs:122-147`

---

## Sistema de Pujas

### Flujo de Puja

```
┌─────────────────────────────────────────────────────────────────┐
│                    FLUJO DE PUJA                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Usuario autenticado                                            │
│       │                                                         │
│       ▼                                                         │
│  POST /api/v1/bids                                             │
│  { "auctionId": 1, "amount": 15600 }                           │
│       │                                                         │
│       ▼                                                         │
│  ┌─────────────────────────────────┐                           │
│  │   INICIAR TRANSACCIÓN           │                           │
│  └─────────────────┬───────────────┘                           │
│                    │                                            │
│                    ▼                                            │
│  ┌─────────────────────────────────┐                           │
│  │   VALIDACIONES                  │                           │
│  │   ├─ ¿Subasta existe?          │                           │
│  │   ├─ ¿Status == Active?         │                           │
│  │   ├─ ¿EndTime > Now?            │                           │
│  │   ├─ ¿Amount >= MinimumBid?     │                           │
│  │   └─ ¿Usuario != CurrentBidder? │                           │
│  └─────────────────┬───────────────┘                           │
│                    │                                            │
│          ┌────────┴────────┐                                    │
│          │ Validación OK   │                                    │
│          └────────┬────────┘                                    │
│                   │                                             │
│                   ▼                                             │
│  ┌─────────────────────────────────┐                           │
│  │   CREAR BID                     │                           │
│  │   + Actualizar Auction          │                           │
│  │     - CurrentBid = Amount       │                           │
│  │     - CurrentBidderId = UserId  │                           │
│  │     - TotalBids++               │                           │
│  └─────────────────┬───────────────┘                           │
│                    │                                            │
│                    ▼                                            │
│  ┌─────────────────────────────────┐                           │
│  │   ¿EXTENDER TIEMPO?             │                           │
│  │   Si (EndTime - Now) <= 2 min   │                           │
│  │   → EndTime += 5 min            │                           │
│  └─────────────────┬───────────────┘                           │
│                    │                                            │
│                    ▼                                            │
│  ┌─────────────────────────────────┐                           │
│  │   COMMIT TRANSACCIÓN            │                           │
│  └─────────────────┬───────────────┘                           │
│                    │                                            │
│       ┌────────────┼────────────┐                               │
│       │            │            │                               │
│       ▼            ▼            ▼                               │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐                         │
│  │ Notif.  │  │ SignalR │  │Response │                         │
│  │ Outbid  │  │ Emit    │  │ HTTP    │                         │
│  │ (async) │  │         │  │         │                         │
│  └─────────┘  └─────────┘  └─────────┘                         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Ubicación del código**: `BidService.cs:29-130`

### Validaciones de Puja

| Validación | Error | Código |
|------------|-------|--------|
| Subasta no existe | `NotFoundException` | 404 |
| Subasta no activa | "La subasta no está activa" | 400 |
| Subasta terminada | "La subasta ha terminado" | 400 |
| Monto insuficiente | "El monto mínimo de puja es {X}" | 400 |
| Ya es el postor actual | "Ya eres el postor actual" | 400 |

### Cálculo del Monto Mínimo

```csharp
// BidService.cs:55-59
var minimumBid = auction.CurrentBid + auction.MinimumBidIncrement;
if (request.Amount < minimumBid)
{
    throw new BadRequestException($"El monto mínimo de puja es {minimumBid:C}");
}
```

**Ejemplo:**
- CurrentBid: $15,000
- MinimumBidIncrement: $100
- **Monto mínimo aceptado**: $15,100

---

## Extensión Automática de Tiempo (Anti-Snipe)

### Concepto

El "sniping" es cuando un postor espera hasta los últimos segundos para pujar, sin dar tiempo a otros a responder. El sistema anti-snipe extiende el tiempo cuando se recibe una puja cerca del cierre.

### Configuración

| Parámetro | Default | Descripción |
|-----------|---------|-------------|
| `ExtensionThresholdMinutes` | 2 | Si quedan ≤ 2 minutos, extender |
| `ExtensionMinutes` | 5 | Extender 5 minutos |

### Lógica de Extensión

```csharp
// BidService.cs:85-94
var timeRemaining = auction.EndTime - DateTime.UtcNow;
var timeExtended = false;
DateTime? newEndTime = null;

if (timeRemaining.TotalMinutes <= auction.ExtensionThresholdMinutes)
{
    auction.EndTime = DateTime.UtcNow.AddMinutes(auction.ExtensionMinutes);
    timeExtended = true;
    newEndTime = auction.EndTime;
}
```

### Diagrama de Extensión

```
┌─────────────────────────────────────────────────────────────────┐
│                   EXTENSIÓN DE TIEMPO                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Tiempo original: 18:00:00                                      │
│  Umbral: 2 minutos                                              │
│  Extensión: 5 minutos                                           │
│                                                                 │
│  ─────────────────────────────────────────────────▶ Tiempo     │
│                                                                 │
│  17:57:30    17:58:00           18:00:00                       │
│     │           │                  │                            │
│     │           │    ┌─── Zona de extensión ───┐               │
│     │           │    │             │           │               │
│     │           ▼    ▼             ▼           │               │
│     │        ████████████████████████          │               │
│     │                                          │               │
│     │                                          │               │
│  Sin extensión   Puja a 17:59:30              │               │
│                  → Nuevo fin: 18:04:30         │               │
│                                                                 │
│  Si otra puja llega a 18:03:00:                                │
│  → Nuevo fin: 18:08:00 (se extiende de nuevo)                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Respuesta con Extensión

```json
{
  "success": true,
  "message": "Puja realizada exitosamente",
  "data": {
    "bidId": 51,
    "amount": 15600.00,
    "newCurrentBid": 15600.00,
    "totalBids": 16,
    "newEndTime": "2024-01-15T18:05:00Z",
    "timeExtended": true
  }
}
```

---

## Cierre Automático de Subastas

### Background Service

El `AuctionCloseService` ejecuta cada **60 segundos** para verificar y cerrar subastas expiradas.

```
┌─────────────────────────────────────────────────────────────────┐
│                  AUCTION CLOSE SERVICE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  while (!cancellationToken.IsCancellationRequested)            │
│  {                                                              │
│      ┌─────────────────────────────────────────────┐           │
│      │  1. Buscar subastas:                        │           │
│      │     Status == Active AND EndTime <= Now     │           │
│      └─────────────────────────────────────────────┘           │
│                         │                                       │
│                         ▼                                       │
│      ┌─────────────────────────────────────────────┐           │
│      │  2. Por cada subasta expirada:              │           │
│      │     ├─ Status = Completed                   │           │
│      │     ├─ Verificar ReservePrice               │           │
│      │     ├─ Crear AuctionHistory                 │           │
│      │     ├─ Marcar puja ganadora                 │           │
│      │     └─ Notificar ganador                    │           │
│      └─────────────────────────────────────────────┘           │
│                         │                                       │
│                         ▼                                       │
│      ┌─────────────────────────────────────────────┐           │
│      │  3. Notificar via SignalR                   │           │
│      │     → "AuctionsClosed" a todos los clientes │           │
│      └─────────────────────────────────────────────┘           │
│                         │                                       │
│                         ▼                                       │
│      ┌─────────────────────────────────────────────┐           │
│      │  4. Esperar 60 segundos                     │           │
│      └─────────────────────────────────────────────┘           │
│  }                                                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Ubicación del código**: `AuctionCloseService.cs:24-59`

### Proceso de Cierre

```csharp
// AuctionService.cs:213-262
foreach (var auction in expiredAuctions)
{
    auction.Status = AuctionStatus.Completed;

    // Verificar si se alcanzó el precio de reserva
    var reserveMet = !auction.ReservePrice.HasValue ||
                     auction.CurrentBid >= auction.ReservePrice.Value;

    // Crear historial
    var history = new AuctionHistory
    {
        AuctionId = auction.Id,
        WinnerId = reserveMet ? auction.CurrentBidderId : null,
        FinalPrice = auction.CurrentBid,
        TotalBids = auction.TotalBids,
        UniqueParticipants = await GetUniqueParticipantsCount(auction.Id),
        CompletedAt = DateTime.UtcNow,
        ReserveMet = reserveMet
    };

    // Notificar ganador (solo si reserva alcanzada)
    if (auction.CurrentBidderId.HasValue && reserveMet)
    {
        await _notificationService.NotifyAuctionWonAsync(...);
    }
}
```

---

## Precio de Reserva

### Concepto

El precio de reserva es un monto mínimo confidencial que debe alcanzarse para que la venta sea válida.

### Lógica

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRECIO DE RESERVA                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Caso 1: Sin precio de reserva                                  │
│  ─────────────────────────────────                              │
│  ReservePrice = null                                            │
│  → Ganador = CurrentBidder (siempre que haya pujado)           │
│                                                                 │
│  Caso 2: Con precio de reserva ALCANZADO                       │
│  ─────────────────────────────────────────                      │
│  ReservePrice = $15,000                                         │
│  CurrentBid = $16,500                                           │
│  → ReserveMet = true                                            │
│  → Ganador = CurrentBidder                                      │
│  → WinnerId en AuctionHistory = CurrentBidderId                │
│                                                                 │
│  Caso 3: Con precio de reserva NO ALCANZADO                    │
│  ─────────────────────────────────────────────                  │
│  ReservePrice = $15,000                                         │
│  CurrentBid = $12,000                                           │
│  → ReserveMet = false                                           │
│  → Sin ganador                                                  │
│  → WinnerId en AuctionHistory = null                           │
│  → No se envía notificación de victoria                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Concurrencia

### Problema

Múltiples usuarios pueden intentar pujar simultáneamente. Sin control, podría haber:
- Condiciones de carrera
- Pujas duplicadas
- Datos inconsistentes

### Solución: Transacciones

```csharp
// BidService.cs:32-129
await using var transaction = await _context.Database.BeginTransactionAsync();

try
{
    // ... validaciones y operaciones ...

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Flujo de Concurrencia

```
Usuario A (Puja $15,100)        Usuario B (Puja $15,200)
         │                               │
         ▼                               ▼
┌─────────────────┐             ┌─────────────────┐
│ BEGIN TRANS (A) │             │ BEGIN TRANS (B) │
└────────┬────────┘             └────────┬────────┘
         │                               │
         ▼                               │
┌─────────────────┐                      │
│ Lock Auction    │                      │
│ CurrentBid=$15K │                      │
└────────┬────────┘                      │
         │                               │
         ▼                               ▼
┌─────────────────┐             ┌─────────────────┐
│ Validar: OK     │             │ ESPERA (locked) │
│ $15,100 >= min  │             │                 │
└────────┬────────┘             └─────────────────┘
         │
         ▼
┌─────────────────┐
│ Update:         │
│ CurrentBid=$15.1│
│ COMMIT          │
└────────┬────────┘
         │                               │
         │                               ▼
         │                      ┌─────────────────┐
         │                      │ Lock Auction    │
         │                      │ CurrentBid=$15.1│
         │                      └────────┬────────┘
         │                               │
         │                               ▼
         │                      ┌─────────────────┐
         │                      │ Validar: OK     │
         │                      │ $15,200 >= min  │
         │                      └────────┬────────┘
         │                               │
         │                               ▼
         │                      ┌─────────────────┐
         │                      │ Update:         │
         │                      │ CurrentBid=$15.2│
         │                      │ COMMIT          │
         │                      └─────────────────┘
```

---

## Sistema de Notificaciones

### Tipos de Notificación

| Tipo | Trigger | Destinatario |
|------|---------|--------------|
| `Outbid` | Nueva puja supera la anterior | Postor anterior |
| `AuctionWon` | Subasta cerrada con reserva | Ganador |
| `AuctionEnding` | Subasta por terminar | Todos los postores |
| `AuctionCancelled` | Admin cancela subasta | Todos los postores |
| `BidPlaced` | Confirmación de puja | Postor actual |

### Flujo de Notificación

```
┌─────────────────────────────────────────────────────────────────┐
│                  NOTIFICACIÓN "OUTBID"                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Usuario B puja $15,200 (supera a Usuario A)                │
│                                                                 │
│  2. BidService detecta previousBidderId = Usuario A            │
│                                                                 │
│  3. Notificación async (no bloquea respuesta):                 │
│     _ = Task.Run(async () => {                                 │
│         await notificationService.NotifyOutbidAsync(...);       │
│     });                                                         │
│                                                                 │
│  4. NotificationService:                                        │
│     ├─ Crear registro en DB (Notifications)                    │
│     └─ Enviar email (async, via EmailService)                  │
│                                                                 │
│  5. Usuario A recibe:                                           │
│     ├─ Notificación in-app (persistida en DB)                  │
│     └─ Email (si implementado)                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Ubicación del código**: `NotificationService.cs:35-57`

---

## Comunicación en Tiempo Real (SignalR)

### Eventos Emitidos

#### BidPlaced

Emitido cuando se coloca una puja válida.

```csharp
// BidsController.cs:35-43
await _hubContext.Clients.Group($"auction_{request.AuctionId}")
    .SendAsync("BidPlaced", new
    {
        auctionId = request.AuctionId,
        currentBid = result.NewCurrentBid,
        totalBids = result.TotalBids,
        newEndTime = result.NewEndTime,
        timeExtended = result.TimeExtended
    });
```

**Destino**: Solo clientes suscritos a `auction_{id}`

#### AuctionsClosed

Emitido cuando el background service cierra subastas.

```csharp
// AuctionCloseService.cs:57
await _hubContext.Clients.All.SendAsync("AuctionsClosed", closedCount);
```

**Destino**: Todos los clientes conectados

### Grupos SignalR

```
┌─────────────────────────────────────────────────────────────────┐
│                    GRUPOS SIGNALR                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Subasta #1              Subasta #2              Subasta #3    │
│  ┌─────────────┐         ┌─────────────┐         ┌───────────┐ │
│  │ auction_1   │         │ auction_2   │         │ auction_3 │ │
│  │ ─────────── │         │ ─────────── │         │ ───────── │ │
│  │ Usuario A   │         │ Usuario C   │         │ Usuario A │ │
│  │ Usuario B   │         │ Usuario D   │         │ Usuario E │ │
│  │ Usuario C   │         │ Usuario E   │         │           │ │
│  └─────────────┘         └─────────────┘         └───────────┘ │
│                                                                 │
│  Un usuario puede estar en múltiples grupos                    │
│  (observando varias subastas simultáneamente)                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## AuctionHistory (Historial)

### Datos Almacenados

```csharp
var history = new AuctionHistory
{
    AuctionId = auction.Id,
    WinnerId = reserveMet ? auction.CurrentBidderId : null,
    FinalPrice = auction.CurrentBid,
    TotalBids = auction.TotalBids,
    UniqueParticipants = uniqueParticipants,
    CompletedAt = DateTime.UtcNow,
    ReserveMet = reserveMet,
    Notes = null  // Para notas administrativas
};
```

### Métricas Capturadas

| Campo | Descripción |
|-------|-------------|
| `FinalPrice` | Precio final (última puja) |
| `TotalBids` | Número total de pujas |
| `UniqueParticipants` | Usuarios únicos que pujaron |
| `ReserveMet` | Si se alcanzó el precio de reserva |
| `CompletedAt` | Timestamp exacto de cierre |

---

## Reglas de Negocio Implementadas

### ✅ Implementadas

1. **Una subasta por carro activa**: No se puede crear subasta si el carro ya tiene una activa
2. **Puja mínima**: CurrentBid + MinimumBidIncrement
3. **No auto-puja**: Usuario no puede superar su propia puja
4. **Extensión anti-snipe**: Configurable por subasta
5. **Precio de reserva**: Opcional, determina validez de ganador
6. **Cierre automático**: Background service cada 60 segundos
7. **Notificaciones**: Outbid, Won, Cancelled
8. **Historial**: Registro completo al cerrar

### ⚠️ Recomendaciones Pendientes

1. **Depósito de garantía**: Requerir depósito antes de pujar
2. **Límite de pujas por usuario**: Evitar spam de pujas
3. **Blacklist de usuarios**: Bloquear usuarios problemáticos
4. **Pujas proxy/automáticas**: Sistema de pujas hasta un máximo
5. **Notificación de inicio**: Avisar cuando inicia una subasta
6. **Countdown en servidor**: Sincronización de tiempo más precisa

---

## Archivos Relacionados

| Archivo | Ubicación | Responsabilidad |
|---------|-----------|-----------------|
| AuctionService.cs | Infrastructure/Services | Lógica de subastas |
| BidService.cs | Infrastructure/Services | Lógica de pujas |
| NotificationService.cs | Infrastructure/Services | Notificaciones |
| AuctionCloseService.cs | API/BackgroundServices | Cierre automático |
| AuctionHub.cs | API/Hubs | SignalR real-time |
| BidsController.cs | API/Controllers | Endpoint de pujas |
| Auction.cs | Domain/Entities | Entidad subasta |
| Bid.cs | Domain/Entities | Entidad puja |
| AuctionHistory.cs | Domain/Entities | Entidad historial |
