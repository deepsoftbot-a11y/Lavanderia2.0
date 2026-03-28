using MediatR;
using LaundryManagement.Application.Queries.Orders;

namespace LaundryManagement.Application.Commands.Orders;

public record UpdateOrderPaymentTotalsCommand(int OrderId) : IRequest<OrderResponseDto>;
