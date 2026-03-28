using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Interfaces;
using MediatR;

namespace LaundryManagement.Application.Queries.Payments;

public record GetPaymentsByOrderIdQuery(int OrderId) : IRequest<List<OrderPaymentDto>>;

public sealed class GetPaymentsByOrderIdQueryHandler : IRequestHandler<GetPaymentsByOrderIdQuery, List<OrderPaymentDto>>
{
    private readonly IPagoService _pagoService;

    public GetPaymentsByOrderIdQueryHandler(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    public async Task<List<OrderPaymentDto>> Handle(GetPaymentsByOrderIdQuery query, CancellationToken cancellationToken)
    {
        return await _pagoService.GetPaymentsByOrderIdAsync(query.OrderId);
    }
}
