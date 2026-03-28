using LaundryManagement.Application.DTOs.Orders;

namespace LaundryManagement.Application.Queries.Orders;

/// <summary>
/// DTO de respuesta completo para el módulo /api/orders.
/// Estructura alineada con el tipo Order del frontend (incluye relaciones pobladas).
/// </summary>
public sealed record OrderResponseDto
{
    public int Id { get; init; }
    public string FolioOrden { get; init; } = string.Empty;

    public int ClientId { get; init; }
    public string PromisedDate { get; init; } = string.Empty;
    public string? DeliveredDate { get; init; }
    public int ReceivedBy { get; init; }
    public int InitialStatusId { get; init; }
    public int OrderStatusId { get; init; }
    public string Notes { get; init; } = string.Empty;
    public string StorageLocation { get; init; } = string.Empty;

    public decimal Subtotal { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal Total { get; init; }

    // Campos de pago (calculados desde tabla Pagos)
    public decimal AmountPaid { get; init; }
    public decimal Balance { get; init; }
    public string PaymentStatus { get; init; } = "pending";

    // Auditoría
    public string CreatedAt { get; init; } = string.Empty;
    public int CreatedBy { get; init; }
    public string? UpdatedAt { get; init; }
    public int? UpdatedBy { get; init; }

    // Items de la orden
    public List<OrderItemResponseDto> Items { get; init; } = new();

    // Relaciones pobladas (para UI — mirrors frontend Order type)
    public OrderClientDto? Client { get; init; }
    public List<OrderPaymentDto> Payments { get; init; } = new();
}

/// <summary>
/// Info básica del cliente incluida en la respuesta de orden.
/// Mapeado al tipo Customer del frontend.
/// </summary>
public sealed record OrderClientDto
{
    public int Id { get; init; }
    public string CustomerNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Rfc { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CurrentBalance { get; init; }
    public bool IsActive { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public int CreatedBy { get; init; }
}

public sealed record OrderItemResponseDto
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

    // Relaciones pobladas (mirrors frontend OrderItem type)
    public OrderItemServiceDto? Service { get; init; }
    public OrderItemGarmentTypeDto? GarmentType { get; init; }
}

public sealed record OrderItemServiceDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public OrderItemCategoryDto? Category { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ChargeType { get; init; } = string.Empty;
    public decimal? PricePerKg { get; init; }
    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public string? Description { get; init; }
    public decimal? EstimatedTime { get; init; }
    public bool IsActive { get; init; }
}

public sealed record OrderItemCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed record OrderItemGarmentTypeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
