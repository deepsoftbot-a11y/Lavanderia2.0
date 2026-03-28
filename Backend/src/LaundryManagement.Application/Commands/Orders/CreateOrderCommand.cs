using MediatR;

namespace LaundryManagement.Application.Commands.Orders;

/// <summary>
/// DTO para el pago inicial opcional al crear una orden
/// </summary>
public sealed record CreateOrderInitialPaymentDto
{
    /// <summary>
    /// Monto del pago inicial (debe ser mayor a 0 y no exceder el total de la orden)
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// ID del método de pago (FK a MetodosPago)
    /// </summary>
    public int PaymentMethodId { get; init; }

    /// <summary>
    /// Referencia del pago (opcional, requerida si el método la requiere)
    /// </summary>
    public string? Reference { get; init; }

    /// <summary>
    /// Observaciones del pago
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Command para crear una nueva orden usando DDD
/// </summary>
public sealed record CreateOrderCommand : IRequest<CreateOrderResult>
{
    public int ClientId { get; init; }
    public DateTime PromisedDate { get; init; }
    public int ReceivedBy { get; init; }
    public int InitialStatusId { get; init; }
    public string? Notes { get; init; }
    public string? StorageLocation { get; init; }
    public List<CreateOrderLineItemDto> Items { get; init; } = new();

    /// <summary>
    /// Pago inicial opcional al crear la orden
    /// </summary>
    public CreateOrderInitialPaymentDto? InitialPayment { get; init; }
}

/// <summary>
/// DTO para los items de la orden en el comando de creación
/// </summary>
public sealed record CreateOrderLineItemDto
{
    public int ServiceId { get; init; }
    public int? ServiceGarmentId { get; init; }
    public decimal? WeightKilos { get; init; }
    public int? Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal? DiscountAmount { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Resultado del comando
/// </summary>
public sealed record CreateOrderResult
{
    public int OrderId { get; init; }
    public string Folio { get; init; } = string.Empty;
    public decimal Total { get; init; }

    // Información del pago inicial (null si no se proporcionó)
    public int? PaymentId { get; init; }
    public decimal? PaidAmount { get; init; }
    public decimal? RemainingBalance { get; init; }
}
