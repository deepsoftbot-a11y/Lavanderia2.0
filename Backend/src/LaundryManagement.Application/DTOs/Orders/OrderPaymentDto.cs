namespace LaundryManagement.Application.DTOs.Orders;

/// <summary>
/// DTO simplificado de pago para incluir en la respuesta de órdenes.
/// Mapeado al tipo Payment del frontend (sin relaciones anidadas).
/// </summary>
public record OrderPaymentDto
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public decimal Amount { get; init; }
    public int PaymentMethodId { get; init; }
    public string? Reference { get; init; }
    public string? Notes { get; init; }
    public string PaidAt { get; init; } = string.Empty;
    public int ReceivedBy { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public int CreatedBy { get; init; }
}
