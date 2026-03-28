using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.DTOs.Payments;
using LaundryManagement.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Payments;

public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, OrderPaymentDto>
{
    private readonly IPagoService _pagoService;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(IPagoService pagoService, ILogger<CreatePaymentCommandHandler> logger)
    {
        _pagoService = pagoService;
        _logger = logger;
    }

    public async Task<OrderPaymentDto> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var request = new CreatePaymentRequest
        {
            OrderId         = command.OrderId,
            Amount          = command.Amount,
            PaymentMethodId = command.PaymentMethodId,
            Reference       = command.Reference,
            Notes           = command.Notes,
            PaidAt          = command.PaidAt,
            ReceivedBy      = command.ReceivedBy,
        };

        var payment = await _pagoService.CreatePaymentAsync(request);

        _logger.LogInformation(
            "Pago {PaymentId} registrado para orden {OrderId} por ${Amount}",
            payment.Id, command.OrderId, command.Amount);

        return payment;
    }
}
