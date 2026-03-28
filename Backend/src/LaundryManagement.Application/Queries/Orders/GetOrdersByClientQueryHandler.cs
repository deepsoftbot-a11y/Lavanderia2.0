using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

/// <summary>
/// Handler para obtener órdenes por cliente usando el patrón DDD
/// </summary>
public sealed class GetOrdersByClientQueryHandler : IRequestHandler<GetOrdersByClientQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByClientQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByClientQuery query, CancellationToken cancellationToken)
    {
        // Obtener los agregados desde el repositorio de dominio
        var orders = await _orderRepository.GetByClientAsync(
            ClientId.From(query.ClientId),
            cancellationToken
        );

        // Filtrar entregadas si es necesario
        var filteredOrders = query.IncludeDelivered
            ? orders
            : orders.Where(o => !o.IsDelivered);

        // Mapear los agregados a DTOs
        return filteredOrders.Select(order => new OrderDto
        {
            OrderId = order.Id.Value,
            Folio = order.Folio.Value,
            ClientId = order.ClientId.Value,
            ReceivedDate = order.ReceivedDate,
            PromisedDate = order.PromisedDate,
            DeliveryDate = order.DeliveryDate,
            StatusId = order.StatusId,
            Subtotal = order.Subtotal.Amount,
            TotalDiscount = order.TotalDiscount.Amount,
            Total = order.Total.Amount,
            Notes = order.Notes,
            StorageLocation = order.StorageLocation,
            ReceivedBy = order.ReceivedBy,
            DeliveredBy = order.DeliveredBy,
            IsDelivered = order.IsDelivered,
            LineItems = order.LineItems.Select(item => new OrderLineItemDto
            {
                LineItemId = item.Id,
                LineNumber = item.LineNumber,
                ServiceId = item.ServiceId,
                ServiceGarmentId = item.ServiceGarmentId,
                WeightKilos = item.WeightKilos,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                Subtotal = item.Subtotal.Amount,
                LineDiscount = item.LineDiscount.Amount,
                LineTotal = item.LineTotal.Amount,
                Notes = item.Notes
            }).ToList(),
            Discounts = order.Discounts.Select(discount => new OrderDiscountDto
            {
                DiscountRecordId = discount.Id,
                DiscountId = discount.DiscountId,
                ComboId = discount.ComboId,
                DiscountAmount = discount.DiscountAmount.Amount,
                Justification = discount.Justification,
                AppliedBy = discount.AppliedBy
            }).ToList()
        })
        .OrderByDescending(o => o.ReceivedDate)
        .ToList();
    }
}
