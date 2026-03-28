using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Notifications;
using LaundryManagement.Application.Queries.Orders;
using LaundryManagement.Domain.DomainEvents.Orders;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.EventHandlers.Orders;

/// <summary>
/// Maneja el evento OrderCreated publicado post-commit.
/// Los fallos de impresión se capturan y loguean — nunca se propagan al caller.
/// Auto-descubierto por MediatR desde RegisterServicesFromAssembly en Application/DI.
/// </summary>
public sealed class PrintTicketOnOrderCreatedHandler
    : INotificationHandler<DomainEventNotification<OrderCreated>>
{
    private readonly ISender _sender;
    private readonly ITicketPrinterService _printer;
    private readonly ILogger<PrintTicketOnOrderCreatedHandler> _logger;

    public PrintTicketOnOrderCreatedHandler(
        ISender sender,
        ITicketPrinterService printer,
        ILogger<PrintTicketOnOrderCreatedHandler> logger)
    {
        _sender = sender;
        _printer = printer;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<OrderCreated> notification,
        CancellationToken cancellationToken)
    {
        var orderId = notification.DomainEvent.OrderId;
        _logger.LogInformation("Procesando OrderCreated para impresión. OrdenID={OrderId}", orderId);

        try
        {
            var order = await _sender.Send(new GetOrderResponseByIdQuery(orderId), cancellationToken);
            await _printer.PrintOrderTicketAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error en handler de impresión para OrdenID={OrderId}. El registro NO fue afectado.", orderId);
        }
    }
}
