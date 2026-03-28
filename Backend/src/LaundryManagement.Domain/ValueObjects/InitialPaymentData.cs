namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Datos del pago inicial asociado a una orden.
/// Este es un simple DTO para transferir datos de pago entre capas sin acoplamientos.
/// </summary>
public record InitialPaymentData(
    decimal Amount,
    int ReceivedBy,
    string PaymentMethodsJson,
    string? Notes
);
