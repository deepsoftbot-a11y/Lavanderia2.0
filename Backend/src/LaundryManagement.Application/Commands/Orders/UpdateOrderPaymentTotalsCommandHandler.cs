using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Queries.Orders;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Orders;

public sealed class UpdateOrderPaymentTotalsCommandHandler
    : IRequestHandler<UpdateOrderPaymentTotalsCommand, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;

    public UpdateOrderPaymentTotalsCommandHandler(IOrderRepository orderRepository, IPagoService pagoService)
    {
        _orderRepository = orderRepository;
        _pagoService = pagoService;
    }

    public async Task<OrderResponseDto> Handle(UpdateOrderPaymentTotalsCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(OrderId.From(command.OrderId), cancellationToken)
            ?? throw new NotFoundException($"Orden con ID {command.OrderId} no encontrada");

        var amountPaid = await _pagoService.GetAmountPaidByOrderAsync(command.OrderId);
        return GetOrdersQueryHandler.MapToDto(order, amountPaid);
    }
}
