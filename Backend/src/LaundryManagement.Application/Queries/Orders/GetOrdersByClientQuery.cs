using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

/// <summary>
/// Query para obtener todas las órdenes de un cliente
/// </summary>
public sealed record GetOrdersByClientQuery : IRequest<List<OrderDto>>
{
    public int ClientId { get; init; }
    public bool IncludeDelivered { get; init; } = true;

    public GetOrdersByClientQuery(int clientId, bool includeDelivered = true)
    {
        ClientId = clientId;
        IncludeDelivered = includeDelivered;
    }
}
