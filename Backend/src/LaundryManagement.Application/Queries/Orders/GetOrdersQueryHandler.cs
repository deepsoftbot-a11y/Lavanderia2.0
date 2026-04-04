using LaundryManagement.Application.Common;
using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;
    private readonly IClientRepository _clientRepository;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        IPagoService pagoService,
        IClientRepository clientRepository)
    {
        _orderRepository = orderRepository;
        _pagoService = pagoService;
        _clientRepository = clientRepository;
    }

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var (ordersEnumerable, totalCount) = await _orderRepository.GetAllAsync(
            search: query.Search,
            clientId: query.ClientId,
            startDate: query.StartDate,
            endDate: query.EndDate,
            statusIds: query.StatusIds,
            paymentStatuses: query.PaymentStatuses,
            sortBy: query.SortBy,
            sortOrder: query.SortOrder,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken
        );

        var orders = ordersEnumerable.ToList();

        if (!orders.Any())
            return new PagedResult<OrderSummaryDto>(new List<OrderSummaryDto>(), totalCount, query.Page, query.PageSize);

        var orderIds  = orders.Select(o => o.Id.Value).ToList();
        var clientIds = orders.Select(o => o.ClientId).Distinct().ToList();

        // Dapper (stateless) y EF Core en paralelo
        var amountsTask = _pagoService.GetAmountsPaidByOrdersAsync(orderIds);
        var clientsTask = _clientRepository.GetByIdsAsync(clientIds, cancellationToken);

        await Task.WhenAll(amountsTask, clientsTask);

        var amountsPaid = await amountsTask;
        var clientsDict = (await clientsTask).ToDictionary(c => c.Id.Value);

        var data = orders.Select(order =>
        {
            var amountPaid = amountsPaid.TryGetValue(order.Id.Value, out var paid) ? paid : 0m;
            var paymentStatus = amountPaid >= order.Total.Amount && order.Total.Amount > 0
                ? "paid"
                : amountPaid > 0 ? "partial" : "pending";

            clientsDict.TryGetValue(order.ClientId.Value, out var client);

            return new OrderSummaryDto
            {
                Id           = order.Id.Value,
                FolioOrden   = order.Folio.Value,
                OrderStatusId = order.StatusId,
                ClientId     = order.ClientId.Value,
                Client       = client == null ? null : new OrderSummaryClientDto { Name = client.Name },
                Total        = order.Total.Amount,
                PaymentStatus = paymentStatus,
                CreatedAt    = order.ReceivedDate.ToString("o"),
            };
        }).ToList();

        return new PagedResult<OrderSummaryDto>(data, totalCount, query.Page, query.PageSize);
    }
}
