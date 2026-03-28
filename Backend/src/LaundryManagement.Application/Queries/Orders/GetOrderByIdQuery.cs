using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

/// <summary>
/// Query para obtener una orden por su ID
/// </summary>
public sealed record GetOrderByIdQuery : IRequest<OrderDto?>
{
    public int OrderId { get; init; }

    public GetOrderByIdQuery(int orderId)
    {
        OrderId = orderId;
    }
}
