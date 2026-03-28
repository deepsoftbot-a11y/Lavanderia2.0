using MediatR;
using LaundryManagement.Application.Queries.Orders;

namespace LaundryManagement.Application.Commands.Orders;

public sealed record UpdateOrderCommand : IRequest<OrderResponseDto>
{
    public int Id { get; init; }
    public DateTime? PromisedDate { get; init; }
    public int? InitialStatusId { get; init; }
    public string? Notes { get; init; }
    public string? StorageLocation { get; init; }
    public List<UpdateOrderLineItemDto>? Items { get; init; }
}

public sealed record UpdateOrderLineItemDto
{
    public int? Id { get; init; }
    public int ServiceId { get; init; }
    public int ServiceGarmentId { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal WeightKilos { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Notes { get; init; } = string.Empty;
}
