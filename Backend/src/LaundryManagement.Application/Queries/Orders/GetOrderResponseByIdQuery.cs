using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.Orders;

public record GetOrderResponseByIdQuery(int OrderId) : IRequest<OrderResponseDto>;

public sealed class GetOrderResponseByIdQueryHandler
    : IRequestHandler<GetOrderResponseByIdQuery, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;
    private readonly IClientRepository _clientRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceGarmentRepository _serviceGarmentRepository;

    public GetOrderResponseByIdQueryHandler(
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

    public async Task<OrderResponseDto> Handle(GetOrderResponseByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(OrderId.From(query.OrderId), cancellationToken)
            ?? throw new NotFoundException($"Orden con ID {query.OrderId} no encontrada");

        var serviceIds = order.LineItems.Select(i => i.ServiceId).Distinct().ToList();
        var servicioPrendaIds = order.LineItems.Where(i => i.ServiceGarmentId.HasValue)
            .Select(i => i.ServiceGarmentId!.Value).Distinct().ToList();

        // Dapper queries son thread-safe: pueden correr en paralelo
        var amountsTask = _pagoService.GetAmountsPaidByOrdersAsync(new[] { query.OrderId });
        var paymentsTask = _pagoService.GetPaymentsByOrdersAsync(new[] { query.OrderId });
        await Task.WhenAll(amountsTask, paymentsTask);

        var amountsPaid = await amountsTask;
        var paymentsByOrder = await paymentsTask;

        // EF Core no soporta múltiples operaciones concurrentes en el mismo DbContext
        var client = await _clientRepository.GetByIdAsync(order.ClientId, cancellationToken);
        var servicesDict = await _serviceRepository.GetByIdsAsync(serviceIds, cancellationToken);
        var garmentTypesDict = await _serviceGarmentRepository.GetGarmentTypesByServicioPrendaIdsAsync(servicioPrendaIds, cancellationToken);

        var amountPaid = amountsPaid.TryGetValue(query.OrderId, out var paid) ? paid : 0m;
        paymentsByOrder.TryGetValue(query.OrderId, out var payments);

        return GetOrdersQueryHandler.MapToDto(order, amountPaid, client, payments, servicesDict, garmentTypesDict);
    }
}
