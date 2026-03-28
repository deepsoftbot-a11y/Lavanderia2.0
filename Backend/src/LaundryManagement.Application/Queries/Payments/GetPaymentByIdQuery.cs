using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using MediatR;

namespace LaundryManagement.Application.Queries.Payments;

public record GetPaymentByIdQuery(int PaymentId) : IRequest<OrderPaymentDto>;

public sealed class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, OrderPaymentDto>
{
    private readonly IPagoService _pagoService;

    public GetPaymentByIdQueryHandler(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    public async Task<OrderPaymentDto> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken)
    {
        var payment = await _pagoService.GetPaymentByIdAsync(query.PaymentId);

        if (payment is null)
            throw new NotFoundException($"Pago con ID {query.PaymentId} no encontrado");

        return payment;
    }
}
