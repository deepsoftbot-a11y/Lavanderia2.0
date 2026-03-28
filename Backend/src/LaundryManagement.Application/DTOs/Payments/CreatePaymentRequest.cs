namespace LaundryManagement.Application.DTOs.Payments;

/// <summary>
/// DTO de entrada para crear un pago desde el frontend.
/// Mapea 1:1 con el tipo CreatePaymentInput del frontend.
/// </summary>
public record CreatePaymentRequest
{
    public int OrderId { get; init; }
    public decimal Amount { get; init; }
    public int PaymentMethodId { get; init; }
    public string? Reference { get; init; }
    public string? Notes { get; init; }
    public string PaidAt { get; init; } = string.Empty;
    public int ReceivedBy { get; init; }
}
