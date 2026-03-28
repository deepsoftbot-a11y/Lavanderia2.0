namespace LaundryManagement.Application.Queries.Orders;

/// <summary>
/// DTO para representar una orden en las queries
/// </summary>
public sealed record OrderDto
{
    public int OrderId { get; init; }
    public string Folio { get; init; } = string.Empty;
    public int ClientId { get; init; }
    public DateTime ReceivedDate { get; init; }
    public DateTime PromisedDate { get; init; }
    public DateTime? DeliveryDate { get; init; }
    public int StatusId { get; init; }
    public decimal Subtotal { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal Total { get; init; }
    public string? Notes { get; init; }
    public string? StorageLocation { get; init; }
    public int ReceivedBy { get; init; }
    public int? DeliveredBy { get; init; }
    public bool IsDelivered { get; init; }
    public List<OrderLineItemDto> LineItems { get; init; } = new();
    public List<OrderDiscountDto> Discounts { get; init; } = new();
}

/// <summary>
/// DTO para items de la orden
/// </summary>
public sealed record OrderLineItemDto
{
    public int LineItemId { get; init; }
    public int LineNumber { get; init; }
    public int ServiceId { get; init; }
    public int? ServiceGarmentId { get; init; }
    public decimal? WeightKilos { get; init; }
    public int? Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
    public decimal LineDiscount { get; init; }
    public decimal LineTotal { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO para descuentos de la orden
/// </summary>
public sealed record OrderDiscountDto
{
    public int DiscountRecordId { get; init; }
    public int? DiscountId { get; init; }
    public int? ComboId { get; init; }
    public decimal DiscountAmount { get; init; }
    public string? Justification { get; init; }
    public int AppliedBy { get; init; }
}
