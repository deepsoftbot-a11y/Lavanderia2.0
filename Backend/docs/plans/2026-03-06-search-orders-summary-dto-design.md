# Design: OrderSummaryDto + Fix deliveredDate

**Date:** 2026-03-06
**Scope:** `GET /orders/search` and `GET /orders/{id}`

## Problem

Two mismatches between frontend types and API responses:

1. `GET /orders/search` returns `List<OrderResponseDto>` (heavy) but frontend expects `OrderSummary[]` (lightweight — no items, no full payments, no full client).
2. `GET /orders/{id}` returns field `deliveryDate` but frontend `Order` type expects `deliveredDate`.

## Solution

### 1. New `OrderSummaryDto`

New file: `Application/DTOs/Orders/OrderSummaryDto.cs`

```csharp
public sealed record OrderSummaryDto
{
    public int Id { get; init; }
    public string FolioOrden { get; init; } = string.Empty;
    public int OrderStatusId { get; init; }
    public int ClientId { get; init; }
    public OrderSummaryClientDto? Client { get; init; }
    public decimal Total { get; init; }
    public string PaymentStatus { get; init; } = "pending";
    public string CreatedAt { get; init; } = string.Empty;
}

public sealed record OrderSummaryClientDto
{
    public string Name { get; init; } = string.Empty;
}
```

### 2. Refactor `SearchOrdersQueryHandler`

- Change `SearchOrdersQuery` return type: `IRequest<List<OrderSummaryDto>>`
- Handler fetches: `GetAllAsync(search)` + `GetAmountsPaidByOrdersAsync` + `GetByIdsAsync` in parallel
- Eliminates `GetPaymentsByOrdersAsync` (not needed for summary)
- Maps directly to `OrderSummaryDto`

### 3. Rename `DeliveryDate` → `DeliveredDate` in `OrderResponseDto`

- `OrderResponseDto.DeliveryDate` → `DeliveredDate`
- `GetOrdersQueryHandler.BuildDto`: update assignment to `DeliveredDate = order.DeliveryDate?.ToString("o")`
- Affects all endpoints returning `OrderResponseDto` (correct — all correspond to frontend `Order` type)

### 4. Controller update

- `OrdersController.SearchOrders`: update `ProducesResponseType` to `List<OrderSummaryDto>`

## Files Changed

| File | Change |
|---|---|
| `Application/DTOs/Orders/OrderSummaryDto.cs` | New |
| `Application/Queries/Orders/SearchOrdersQuery.cs` | Return type → `List<OrderSummaryDto>` |
| `Application/Queries/Orders/SearchOrdersQueryHandler.cs` | Replace logic |
| `Application/Queries/Orders/OrderResponseDto.cs` | Rename `DeliveryDate` → `DeliveredDate` |
| `Application/Queries/Orders/GetOrdersQueryHandler.cs` | Update `BuildDto` assignment |
| `API/Controllers/OrdersController.cs` | Update `ProducesResponseType` |
