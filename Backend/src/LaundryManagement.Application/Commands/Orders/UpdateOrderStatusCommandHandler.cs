using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Queries.Orders;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Orders;

public sealed class UpdateOrderStatusCommandHandler
    : IRequestHandler<UpdateOrderStatusCommand, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IPagoService pagoService,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _pagoService = pagoService;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(OrderId.From(command.OrderId), cancellationToken)
            ?? throw new NotFoundException($"Orden con ID {command.OrderId} no encontrada");

        order.ChangeStatus(command.NewStatusId, command.ChangedBy);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation("Estado de orden {OrderId} cambiado a {NewStatusId}", command.OrderId, command.NewStatusId);

        var amountPaid = await _pagoService.GetAmountPaidByOrderAsync(command.OrderId);
        return OrderResponseMapper.MapToDto(order, amountPaid);
    }
}
