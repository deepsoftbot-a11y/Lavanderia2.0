# Spec: Orders List — Cambio de respuesta a OrderSummaryDto

**Fecha:** 2026-04-04
**Estado:** Aprobado

## Contexto

`GET /api/orders` actualmente retorna `PagedResult<OrderResponseDto>`, un DTO pesado que incluye items con servicios y tipos de prenda poblados, cliente completo y lista de pagos detallada. Esto requiere múltiples queries al DB por cada petición de listado.

`GET /api/orders/search` retorna `List<OrderSummaryDto>`, un DTO ligero que solo incluye los campos necesarios para mostrar una fila en la tabla de órdenes.

El objetivo es que ambos endpoints sean consistentes: el listado paginado usa el mismo shape que la búsqueda rápida.

## Objetivo

Cambiar `GET /api/orders` para retornar `PagedResult<OrderSummaryDto>` en lugar de `PagedResult<OrderResponseDto>`, manteniendo todos los filtros y parámetros de paginación existentes.

## Shape de respuesta

### `OrderSummaryDto` (sin cambios)

```json
{
  "id": 42,
  "folioOrden": "ORD-20260404-0042",
  "orderStatusId": 2,
  "clientId": 5,
  "client": { "name": "Juan Pérez" },
  "total": 170.00,
  "paymentStatus": "partial",
  "createdAt": "2026-04-04T10:00:00"
}
```

### `PagedResult<OrderSummaryDto>`

```json
{
  "items": [ /* OrderSummaryDto[] */ ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

## Query params (sin cambios)

| Param | Tipo | Default |
|-------|------|---------|
| `search` | string? | — |
| `clientId` | int? | — |
| `startDate` | DateTime? | — |
| `endDate` | DateTime? | — |
| `statusIds[]` | int[]? | — |
| `paymentStatuses[]` | string[]? | — |
| `sortBy` | string | `"createdAt"` |
| `sortOrder` | string | `"desc"` |
| `page` | int | `1` |
| `pageSize` | int | `20` |

## Cambios requeridos

### Backend

#### `GetOrdersQuery.cs`
- Cambiar `IRequest<PagedResult<OrderResponseDto>>` → `IRequest<PagedResult<OrderSummaryDto>>`

#### `GetOrdersQueryHandler.cs`
- Cambiar `IRequestHandler<GetOrdersQuery, PagedResult<OrderResponseDto>>` → `PagedResult<OrderSummaryDto>`
- Eliminar dependencias: `IServiceRepository`, `IServiceGarmentRepository`
- Eliminar métodos: `FetchRelatedDataAsync`, `MapToDto` (todas las sobrecargas), `BuildDto`
- Nueva lógica (igual a `SearchOrdersQueryHandler` + paginación):
  1. Llamar `_orderRepository.GetAllAsync(...)` con todos los filtros y paginación
  2. Si lista vacía → retornar `PagedResult` vacío
  3. Cargar en paralelo: `_pagoService.GetAmountsPaidByOrdersAsync` (Dapper) + `_clientRepository.GetByIdsAsync` (EF)
  4. Mapear cada `OrderPure` → `OrderSummaryDto` (mismo cálculo de `paymentStatus`)
  5. Retornar `PagedResult<OrderSummaryDto>`

#### `OrdersController.cs`
- Actualizar `[ProducesResponseType(typeof(PagedResult<OrderResponseDto>))]` → `PagedResult<OrderSummaryDto>`

### Frontend

- Localizar el store/tipos que consumen `GET /api/orders`
- Actualizar el tipo del item de `Order` (shape completo) a `OrderSummary` (shape ligero)
- Verificar que la tabla de órdenes (`OrdersTable`) solo use campos disponibles en `OrderSummaryDto`

## Lo que NO cambia

- Parámetros de query (filtros + paginación)
- `GET /api/orders/search` — sin cambios
- `GET /api/orders/{id}` — sigue retornando `OrderResponseDto` completo
- `OrderSummaryDto` — sin cambios en su estructura

## Decisión de diseño

Se eligió el Enfoque A (reemplazar tipo de retorno) sobre:
- **Enfoque B** (nuevo endpoint paralelo): duplicación innecesaria
- **Enfoque C** (unificar con `/search`): rompe contrato de búsqueda sin beneficio claro
