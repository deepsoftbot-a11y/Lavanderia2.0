# Diseño: Integración API Orders — Frontend ↔ Backend

**Fecha:** 2026-02-16
**Alcance:** Integrar los 7 métodos que el frontend consume en `/src/api/orders/ordersService.api.ts` con el backend .NET 8 DDD.
**Enfoque:** DDD puro (CQRS + MediatR). Sin Dapper en el nuevo módulo.

---

## Contexto y Brechas Identificadas

### Métodos del frontend vs endpoints backend

| Método | URL Frontend | Backend actual | Estado |
|--------|-------------|----------------|--------|
| `getOrders(filters)` | `GET /orders` | ❌ No existe | **Implementar** |
| `getOrderById(id)` | `GET /orders/{id}` | `GET /api/ordenes/{id}` | ⚠️ Ruta diferente |
| `createOrder(input)` | `POST /ordenes/v2` | `POST /api/ordenes/v2` | ✅ Ya existe |
| `updateOrder(id, input)` | `PUT /orders/{id}` | ❌ No existe | **Implementar** |
| `deleteOrder(id)` | `DELETE /orders/{id}` | ❌ No existe | **Implementar** |
| `updateOrderPaymentTotals(id)` | `PATCH /orders/{id}/payment-totals` | ❌ No existe | **Implementar** |
| `searchOrders(filters)` | `GET /orders/search` | ❌ No existe | **Implementar** |
| `updateOrderStatus(id, input)` | `PATCH /orders/{id}/status` | `PUT /api/ordenes/estado` (legacy) | ⚠️ Contrato diferente |

### Mismatch en DTO de respuesta

El `OrderDto` existente no incluye campos que el frontend necesita:
- **Campos de pago:** `amountPaid`, `balance`, `paymentStatus`
- **Naming:** `folioOrden` (frontend) vs `Folio` (backend), `orderStatusId` vs `StatusId`
- **Auditoría:** `createdAt`, `createdBy`, `updatedAt`, `updatedBy`
- **Estado prenda:** `initialStatusId`

---

## Decisiones Clave

| Decisión | Elección |
|----------|----------|
| Nomenclatura rutas | `/api/orders` (inglés, consistente con frontend) |
| Borrado de orden | Lógico — cambia estado a Cancelada |
| Paginación en lista | Sin paginación por ahora |
| Retrocompatibilidad | `OrdenesController` se mantiene intacto |
| Patrón arquitectónico | DDD puro — CQRS + MediatR en todos los endpoints |

---

## Sección 1: Nuevo Controlador `OrdersController`

**Archivo:** `src/LaundryManagement.API/Controllers/OrdersController.cs`
**Ruta base:** `[Route("api/orders")]`

```
GET    /api/orders                      → GetOrdersQuery
GET    /api/orders/{id}                 → GetOrderByIdQuery (reutiliza el existente)
GET    /api/orders/search               → SearchOrdersQuery
PUT    /api/orders/{id}                 → UpdateOrderCommand
DELETE /api/orders/{id}                 → CancelOrderCommand
PATCH  /api/orders/{id}/payment-totals  → UpdateOrderPaymentTotalsCommand
PATCH  /api/orders/{id}/status          → UpdateOrderStatusCommand
```

`OrdenesController` no se toca. Los endpoints `POST /ordenes/v2`, `GET /ordenes/{id}`, `GET /ordenes/client/{clientId}` siguen funcionando.

---

## Sección 2: Nuevo DTO `OrderResponseDto`

**Archivo:** `src/LaundryManagement.Application/Queries/Orders/OrderResponseDto.cs`

```csharp
public record OrderResponseDto
{
    // Identificación
    public int Id { get; init; }
    public string FolioOrden { get; init; } = string.Empty;

    // Datos de la orden
    public int ClientId { get; init; }
    public string PromisedDate { get; init; } = string.Empty;  // ISO 8601
    public string? DeliveryDate { get; init; }
    public int ReceivedBy { get; init; }
    public int InitialStatusId { get; init; }
    public int OrderStatusId { get; init; }
    public string Notes { get; init; } = string.Empty;
    public string StorageLocation { get; init; } = string.Empty;

    // Totales calculados
    public decimal Subtotal { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal Total { get; init; }

    // Campos de pago (calculados desde tabla Pagos via IPagoService)
    public decimal AmountPaid { get; init; }
    public decimal Balance { get; init; }
    public string PaymentStatus { get; init; } = "pending";  // "pending" | "partial" | "paid"

    // Auditoría
    public string CreatedAt { get; init; } = string.Empty;
    public int CreatedBy { get; init; }
    public string? UpdatedAt { get; init; }
    public int? UpdatedBy { get; init; }

    // Items de la orden
    public List<OrderItemResponseDto> Items { get; init; } = new();
}

public record OrderItemResponseDto
{
    public int Id { get; init; }
    public int ServiceId { get; init; }
    public int ServiceGarmentId { get; init; }
    public int DiscountId { get; init; }
    public decimal WeightKilos { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Notes { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
}
```

**Estrategia para campos de pago:** El query handler inyecta `IPagoService` y llama a un nuevo método `GetAmountPaidByOrderAsync(orderId)` (o el equivalente existente) para calcular `amountPaid`. Luego: `balance = total - amountPaid`, `paymentStatus` = "pending" | "partial" | "paid".

---

## Sección 3: Extensiones de Dominio e Infraestructura

### `IOrderRepository` — nuevos métodos
**Archivo:** `src/LaundryManagement.Domain/Repositories/IOrderRepository.cs`

```csharp
// Listar con filtros
Task<IEnumerable<OrderPure>> GetAllAsync(
    string? search,
    int? clientId,
    DateTime? startDate,
    DateTime? endDate,
    string sortBy = "createdAt",
    string sortOrder = "desc",
    CancellationToken ct = default);

// Actualizar (puede ya existir)
Task UpdateAsync(OrderPure order, CancellationToken ct = default);
```

### `OrderPure` — nuevos métodos del agregado
**Archivo:** `src/LaundryManagement.Domain/Aggregates/Orders/OrderPure.cs`

```csharp
// Actualiza datos editables de la orden
public void UpdateDetails(
    DateTime? promisedDate,
    string? notes,
    string? storageLocation);

// Reemplaza los line items completos
// (valida que al menos 1 item, recalcula totales)
public void ReplaceLineItems(
    IEnumerable<(int serviceId, int? garmentId, decimal? weight,
                  int? qty, decimal unitPrice, decimal discount, string? notes)> newItems);

// Cancelación lógica — emite OrderStatusChanged con estado Cancelada
public void Cancel(int cancelledBy);

// Cambio de estado general
public void ChangeStatus(int newStatusId, int changedBy);
```

### `OrderRepositoryPure` — implementación de `GetAllAsync`
**Archivo:** `src/LaundryManagement.Infrastructure/Repositories/OrderRepositoryPure.cs`

Usa EF Core con:
- `.Where()` para filtros de `clientId`, `startDate`, `endDate`
- `.Where()` con `EF.Functions.Like()` para búsqueda en `Folio`
- Join implícito con tabla `Clientes` para filtrar por nombre/teléfono
- `.OrderBy()` / `.OrderByDescending()` según `sortBy` + `sortOrder`
- `.Include(o => o.OrdenesDetalles)` para cargar items

---

## Sección 4: Capa Application — CQRS Completo

### Nuevas Queries

#### `GetOrdersQuery`
**Archivo:** `src/LaundryManagement.Application/Queries/Orders/GetOrdersQuery.cs`

```csharp
public record GetOrdersQuery(
    string? Search,
    int? ClientId,
    DateTime? StartDate,
    DateTime? EndDate,
    string SortBy = "createdAt",
    string SortOrder = "desc"
) : IRequest<List<OrderResponseDto>>;
```

#### `SearchOrdersQuery`
**Archivo:** `src/LaundryManagement.Application/Queries/Orders/SearchOrdersQuery.cs`

```csharp
public record SearchOrdersQuery(string Query) : IRequest<List<OrderResponseDto>>;
// Delega a GetOrdersQuery con Search = Query
```

### Nuevos Commands

#### `UpdateOrderCommand`
```csharp
public record UpdateOrderCommand(
    int Id,
    DateTime? PromisedDate,
    int? InitialStatusId,
    string? Notes,
    string? StorageLocation,
    List<UpdateOrderLineItemDto>? Items
) : IRequest<OrderResponseDto>;

public record UpdateOrderLineItemDto(
    int? Id,
    int ServiceId,
    int ServiceGarmentId,
    decimal DiscountAmount,
    decimal WeightKilos,
    int Quantity,
    decimal UnitPrice,
    string Notes
);
```

#### `CancelOrderCommand`
```csharp
public record CancelOrderCommand(int OrderId, int CancelledBy) : IRequest;
```

#### `UpdateOrderPaymentTotalsCommand`
```csharp
public record UpdateOrderPaymentTotalsCommand(int OrderId) : IRequest<OrderResponseDto>;
```

#### `UpdateOrderStatusCommand`
```csharp
public record UpdateOrderStatusCommand(
    int OrderId,
    int NewStatusId
) : IRequest<OrderResponseDto>;
```

### Validators (FluentValidation)

- `UpdateOrderCommandValidator`: `Id > 0`, `Items` al menos 1 si se proporcionan, `UnitPrice > 0`
- `UpdateOrderStatusCommandValidator`: `OrderId > 0`, `NewStatusId > 0`

---

## Resumen de Archivos a Crear/Modificar

### Crear (nuevos):
```
src/LaundryManagement.API/Controllers/
  └── OrdersController.cs

src/LaundryManagement.Application/Queries/Orders/
  ├── OrderResponseDto.cs
  ├── GetOrdersQuery.cs
  ├── GetOrdersQueryHandler.cs
  ├── SearchOrdersQuery.cs
  └── SearchOrdersQueryHandler.cs

src/LaundryManagement.Application/Commands/Orders/
  ├── UpdateOrderCommand.cs
  ├── UpdateOrderCommandHandler.cs
  ├── UpdateOrderCommandValidator.cs
  ├── CancelOrderCommand.cs
  ├── CancelOrderCommandHandler.cs
  ├── UpdateOrderPaymentTotalsCommand.cs
  ├── UpdateOrderPaymentTotalsCommandHandler.cs
  ├── UpdateOrderStatusCommand.cs
  ├── UpdateOrderStatusCommandHandler.cs
  └── UpdateOrderStatusCommandValidator.cs
```

### Modificar (extensiones):
```
src/LaundryManagement.Domain/
  ├── Aggregates/Orders/OrderPure.cs        (+UpdateDetails, +ReplaceLineItems, +Cancel, +ChangeStatus)
  └── Repositories/IOrderRepository.cs     (+GetAllAsync, +UpdateAsync)

src/LaundryManagement.Infrastructure/
  └── Repositories/OrderRepositoryPure.cs  (+GetAllAsync, +UpdateAsync)

src/LaundryManagement.Application/
  └── Interfaces/IPagoService.cs           (+GetAmountPaidByOrderAsync si no existe)

src/LaundryManagement.Infrastructure/
  └── Services/PagoService.cs              (+GetAmountPaidByOrderAsync si no existe)
```

### No modificar:
```
OrdenesController.cs          (retrocompatibilidad preservada)
OrderDto.cs                   (existente, no se altera)
CreateOrderCommand.cs         (existente, no se altera)
GetOrderByIdQuery.cs          (reutilizado desde OrdersController)
GetOrdersByClientQuery.cs     (sin cambios)
```

---

## Orden de Implementación Recomendado

1. **Domain** — Métodos `UpdateDetails`, `ReplaceLineItems`, `Cancel`, `ChangeStatus` en `OrderPure` + métodos en `IOrderRepository`
2. **Infrastructure** — Implementar `UpdateAsync` y `GetAllAsync` en `OrderRepositoryPure`
3. **Application Queries** — `GetOrdersQueryHandler` (con `IPagoService` para campos de pago), `SearchOrdersQueryHandler`
4. **Application Commands** — `UpdateOrderCommandHandler`, `CancelOrderCommandHandler`, `UpdateOrderStatusCommandHandler`, `UpdateOrderPaymentTotalsCommandHandler`
5. **API** — `OrdersController` con los 7 endpoints
6. **Verificación** — Build + Swagger manual de cada endpoint
