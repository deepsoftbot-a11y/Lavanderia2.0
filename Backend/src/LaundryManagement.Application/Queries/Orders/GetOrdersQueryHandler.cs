using LaundryManagement.Application.Common;
using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.Aggregates.ServiceGarments;
using LaundryManagement.Domain.Aggregates.Services;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;
    private readonly IClientRepository _clientRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceGarmentRepository _serviceGarmentRepository;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        IPagoService pagoService,
        IClientRepository clientRepository,
        IServiceRepository serviceRepository,
        IServiceGarmentRepository serviceGarmentRepository)
    {
        _orderRepository = orderRepository;
        _pagoService = pagoService;
        _clientRepository = clientRepository;
        _serviceRepository = serviceRepository;
        _serviceGarmentRepository = serviceGarmentRepository;
    }

    public async Task<PagedResult<OrderResponseDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetAllAsync(
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

        var ordersList = orders.ToList();

        if (!ordersList.Any())
            return new PagedResult<OrderResponseDto>(new List<OrderResponseDto>(), totalCount, query.Page, query.PageSize);

        var orderIds = ordersList.Select(o => o.Id.Value).ToList();
        var allItems = ordersList.SelectMany(o => o.LineItems).ToList();
        var serviceIds = allItems.Select(i => i.ServiceId).Distinct().ToList();
        var servicioPrendaIds = allItems.Where(i => i.ServiceGarmentId.HasValue)
            .Select(i => i.ServiceGarmentId!.Value).Distinct().ToList();

        var (amountsPaid, paymentsByOrder, clientsDict, servicesDict, garmentTypesDict) =
            await FetchRelatedDataAsync(
                orderIds, ordersList.Select(o => o.ClientId).Distinct(),
                serviceIds, servicioPrendaIds, cancellationToken);

        var data = ordersList
            .Select(order => BuildDto(order, amountsPaid, paymentsByOrder, clientsDict, servicesDict, garmentTypesDict))
            .ToList();

        return new PagedResult<OrderResponseDto>(data, totalCount, query.Page, query.PageSize);
    }

    internal async Task<(Dictionary<int, decimal> amountsPaid,
                          Dictionary<int, List<OrderPaymentDto>> paymentsByOrder,
                          Dictionary<int, ClientPure> clientsDict,
                          Dictionary<int, ServicePure> servicesDict,
                          Dictionary<int, ServiceGarmentPure> garmentTypesDict)>
        FetchRelatedDataAsync(
            IEnumerable<int> orderIds,
            IEnumerable<ClientId> clientIds,
            IEnumerable<int> serviceIds,
            IEnumerable<int> servicioPrendaIds,
            CancellationToken ct)
    {
        var orderIdList = orderIds.ToList();
        var clientIdList = clientIds.ToList();
        var serviceIdList = serviceIds.ToList();
        var garmentIdList = servicioPrendaIds.ToList();

        // Ejecutar consultas en paralelo — Dapper (pagos) y EF Core (clientes, servicios, tipos de prenda)
        var amountsTask      = _pagoService.GetAmountsPaidByOrdersAsync(orderIdList);
        var paymentsTask     = _pagoService.GetPaymentsByOrdersAsync(orderIdList);
        var clientsTask      = _clientRepository.GetByIdsAsync(clientIdList, ct);
        var servicesTask     = serviceIdList.Count == 0
            ? Task.FromResult(new Dictionary<int, ServicePure>())
            : _serviceRepository.GetByIdsAsync(serviceIdList, ct);
        var garmentTypesTask = garmentIdList.Count == 0
            ? Task.FromResult(new Dictionary<int, ServiceGarmentPure>())
            : _serviceGarmentRepository.GetGarmentTypesByServicioPrendaIdsAsync(garmentIdList, ct);

        await Task.WhenAll(amountsTask, paymentsTask, clientsTask, servicesTask, garmentTypesTask);

        var clientsDict = (await clientsTask).ToDictionary(c => c.Id.Value);

        return (await amountsTask, await paymentsTask, clientsDict, await servicesTask, await garmentTypesTask);
    }

    internal static OrderResponseDto MapToDto(OrderPure order, Dictionary<int, decimal> amountsPaid)
    {
        var amountPaid = amountsPaid.TryGetValue(order.Id.Value, out var paid) ? paid : 0m;
        return BuildDto(order, amountsPaid, new Dictionary<int, List<OrderPaymentDto>>(), new Dictionary<int, ClientPure>(),
            new Dictionary<int, ServicePure>(), new Dictionary<int, ServiceGarmentPure>());
    }

    internal static OrderResponseDto MapToDto(OrderPure order, decimal amountPaid)
        => BuildDto(order, amountPaid, null, null, null, null);

    internal static OrderResponseDto MapToDto(
        OrderPure order,
        decimal amountPaid,
        ClientPure? client,
        List<OrderPaymentDto>? payments,
        Dictionary<int, ServicePure>? servicesDict = null,
        Dictionary<int, ServiceGarmentPure>? garmentTypesDict = null)
        => BuildDto(order, amountPaid, client, payments, servicesDict, garmentTypesDict);

    private static OrderResponseDto BuildDto(
        OrderPure order,
        Dictionary<int, decimal> amountsPaid,
        Dictionary<int, List<OrderPaymentDto>> paymentsByOrder,
        Dictionary<int, ClientPure> clientsDict,
        Dictionary<int, ServicePure> servicesDict,
        Dictionary<int, ServiceGarmentPure> garmentTypesDict)
    {
        var amountPaid = amountsPaid.TryGetValue(order.Id.Value, out var paid) ? paid : 0m;
        paymentsByOrder.TryGetValue(order.Id.Value, out var payments);
        clientsDict.TryGetValue(order.ClientId.Value, out var client);

        return BuildDto(order, amountPaid, client, payments, servicesDict, garmentTypesDict);
    }

    private static OrderResponseDto BuildDto(
        OrderPure order,
        decimal amountPaid,
        ClientPure? client,
        List<OrderPaymentDto>? payments,
        Dictionary<int, ServicePure>? servicesDict,
        Dictionary<int, ServiceGarmentPure>? garmentTypesDict)
    {
        var balance = order.Total.Amount - amountPaid;
        var paymentStatus = amountPaid >= order.Total.Amount && order.Total.Amount > 0
            ? "paid"
            : amountPaid > 0 ? "partial" : "pending";

        return new OrderResponseDto
        {
            Id = order.Id.Value,
            FolioOrden = order.Folio.Value,
            ClientId = order.ClientId.Value,
            PromisedDate = order.PromisedDate.ToString("o"),
            DeliveredDate = order.DeliveryDate?.ToString("o"),
            ReceivedBy = order.ReceivedBy,
            InitialStatusId = order.StatusId,
            OrderStatusId = order.StatusId,
            Notes = order.Notes ?? string.Empty,
            StorageLocation = order.StorageLocation ?? string.Empty,
            Subtotal = order.Subtotal.Amount,
            TotalDiscount = order.TotalDiscount.Amount,
            Total = order.Total.Amount,
            AmountPaid = amountPaid,
            Balance = balance,
            PaymentStatus = paymentStatus,
            CreatedAt = order.ReceivedDate.ToString("o"),
            CreatedBy = order.ReceivedBy,
            Items = order.LineItems.Select(item =>
            {
                ServicePure? service = null;
                servicesDict?.TryGetValue(item.ServiceId, out service);
                ServiceGarmentPure? garmentType = null;
                if (item.ServiceGarmentId.HasValue)
                    garmentTypesDict?.TryGetValue(item.ServiceGarmentId.Value, out garmentType);

                return new OrderItemResponseDto
                {
                    Id = item.Id,
                    ServiceId = item.ServiceId,
                    ServiceGarmentId = item.ServiceGarmentId ?? 0,
                    DiscountId = 0,
                    WeightKilos = item.WeightKilos ?? 0m,
                    Quantity = item.Quantity ?? 0,
                    UnitPrice = item.UnitPrice.Amount,
                    Notes = item.Notes ?? string.Empty,
                    Subtotal = item.Subtotal.Amount,
                    DiscountAmount = item.LineDiscount.Amount,
                    Total = item.LineTotal.Amount,
                    Service = service == null ? null : new OrderItemServiceDto
                    {
                        Id = service.Id.Value,
                        CategoryId = service.CategoryId.Value,
                        Category = new OrderItemCategoryDto
                        {
                            Id = service.Category.Id.Value,
                            Name = service.Category.Name,
                            IsActive = true
                        },
                        Code = service.Code.Value,
                        Name = service.Name,
                        ChargeType = service.IsPieceBased ? "PorPieza" : "PorPeso",
                        PricePerKg = service.WeightPricing?.PricePerKilo.Amount,
                        MinWeight = service.WeightPricing?.MinimumWeight,
                        MaxWeight = service.WeightPricing?.MaximumWeight,
                        Description = service.Description,
                        EstimatedTime = service.EstimatedHours,
                        IsActive = service.IsActive
                    },
                    GarmentType = garmentType == null ? null : new OrderItemGarmentTypeDto
                    {
                        Id = garmentType.Id.Value,
                        Name = garmentType.Name,
                        Description = garmentType.Description,
                        IsActive = garmentType.IsActive
                    }
                };
            }).ToList(),
            Client = client == null ? null : new OrderClientDto
            {
                Id = client.Id.Value,
                CustomerNumber = client.CustomerNumber,
                Name = client.Name,
                Phone = client.PhoneNumber.Value,
                Email = client.Email?.Value,
                Address = client.Address,
                Rfc = client.Rfc?.Value,
                CreditLimit = client.CreditLimit.Amount,
                CurrentBalance = client.CurrentBalance.Amount,
                IsActive = client.IsActive,
                CreatedAt = client.RegisteredAt.ToString("o"),
                CreatedBy = client.RegisteredBy
            },
            Payments = payments ?? new List<OrderPaymentDto>()
        };
    }
}
