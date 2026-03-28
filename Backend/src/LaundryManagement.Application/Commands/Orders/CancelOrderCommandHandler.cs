using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Orders;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(OrderId.From(command.OrderId), cancellationToken)
            ?? throw new NotFoundException($"Orden con ID {command.OrderId} no encontrada");

        order.Cancel(command.CancelledBy);

        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation("Orden {OrderId} cancelada por usuario {UserId}", command.OrderId, command.CancelledBy);
    }
}
