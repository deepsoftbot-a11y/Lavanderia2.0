using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public sealed class SearchOrdersQueryHandler : IRequestHandler<SearchOrdersQuery, List<OrderSummaryDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;
    private readonly IClientRepository _clientRepository;

    public SearchOrdersQueryHandler(
        IOrderRepository orderRepository,
        IPagoService pagoService,
        IClientRepository clientRepository)
    {
        _orderRepository = orderRepository;
        _pagoService = pagoService;
        _clientRepository = clientRepository;
    }

    public async Task<List<OrderSummaryDto>> Handle(SearchOrdersQuery query, CancellationToken cancellationToken)
    {
        var (ordersEnumerable, _) = await _orderRepository.GetAllAsync(
            search: query.Query,
            cancellationToken: cancellationToken
        );
        var orders = ordersEnumerable.ToList();

        if (!orders.Any())
            return new List<OrderSummaryDto>();

        var orderIds = orders.Select(o => o.Id.Value).ToList();
        var clientIds = orders.Select(o => o.ClientId).Distinct().ToList();

        var amountsTask = _pagoService.GetAmountsPaidByOrdersAsync(orderIds);
        var clientsTask = _clientRepository.GetByIdsAsync(clientIds, cancellationToken);

        await Task.WhenAll(amountsTask, clientsTask);

        var amountsPaid = await amountsTask;
        var clientsDict = (await clientsTask).ToDictionary(c => c.Id.Value);

        return orders.Select(order =>
        {
            var amountPaid = amountsPaid.TryGetValue(order.Id.Value, out var paid) ? paid : 0m;
            var paymentStatus = amountPaid >= order.Total.Amount && order.Total.Amount > 0
                ? "paid"
                : amountPaid > 0 ? "partial" : "pending";

            clientsDict.TryGetValue(order.ClientId.Value, out var client);

            return new OrderSummaryDto
            {
                Id = order.Id.Value,
                FolioOrden = order.Folio.Value,
                OrderStatusId = order.StatusId,
                ClientId = order.ClientId.Value,
                Client = client == null ? null : new OrderSummaryClientDto { Name = client.Name },
                Total = order.Total.Amount,
                PaymentStatus = paymentStatus,
                CreatedAt = order.ReceivedDate.ToString("o")
            };
        }).ToList();
    }
}
