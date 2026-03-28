namespace LaundryManagement.Application.DTOs.Orders;

/// <summary>
/// DTO ligero para resultados de búsqueda (GET /orders/search).
/// Alineado con el tipo OrderSummary del frontend.
/// </summary>
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
