using LaundryManagement.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Payments;

public sealed class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand>
{
    private readonly IPagoService _pagoService;
    private readonly ILogger<CancelPaymentCommandHandler> _logger;

    public CancelPaymentCommandHandler(IPagoService pagoService, ILogger<CancelPaymentCommandHandler> logger)
    {
        _pagoService = pagoService;
        _logger = logger;
    }

    public async Task Handle(CancelPaymentCommand command, CancellationToken cancellationToken)
    {
        await _pagoService.CancelPaymentAsync(command.PaymentId, command.CancelledBy);

        _logger.LogInformation(
            "Pago {PaymentId} cancelado por usuario {UserId}",
            command.PaymentId, command.CancelledBy);
    }
}
