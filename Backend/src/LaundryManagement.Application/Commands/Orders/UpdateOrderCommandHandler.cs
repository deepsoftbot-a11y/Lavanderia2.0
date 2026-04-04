using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Queries.Orders;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Orders;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPagoService _pagoService;
    private readonly ILogger<UpdateOrderCommandHandler> _logger;

    public UpdateOrderCommandHandler(
        IOrderRepository orderRepository,
        IPagoService pagoService,
        ILogger<UpdateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _pagoService = pagoService;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(OrderId.From(command.Id), cancellationToken)
            ?? throw new NotFoundException($"Orden con ID {command.Id} no encontrada");

        order.UpdateDetails(command.PromisedDate, command.Notes, command.StorageLocation);

        var hasNewItems = command.Items is { Count: > 0 };
        if (hasNewItems)
        {
            var newItems = command.Items!.Select(item => (
                serviceId: item.ServiceId,
                serviceGarmentId: item.ServiceGarmentId == 0 ? (int?)null : item.ServiceGarmentId,
                weightKilos: item.WeightKilos > 0 ? (decimal?)item.WeightKilos : null,
                quantity: item.Quantity > 0 ? (int?)item.Quantity : null,
                unitPrice: Money.FromDecimal(item.UnitPrice),
                lineDiscount: item.DiscountAmount > 0
                    ? Money.FromDecimal(item.DiscountAmount)
                    : Money.Zero(),
                notes: string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes
            ));

            order.ReplaceLineItems(newItems);
        }

        if (hasNewItems)
        {
            // Reemplaza line items + descuentos + actualiza escalares en una sola operación
            await _orderRepository.ReplaceLineItemsAsync(order, cancellationToken);
        }
        else
        {
            // Solo actualiza propiedades escalares (fechas, notas, ubicación)
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        _logger.LogInformation("Orden {OrderId} actualizada exitosamente", command.Id);

        var amountPaid = await _pagoService.GetAmountPaidByOrderAsync(command.Id);
        return OrderResponseMapper.MapToDto(order, amountPaid);
    }
}
