using LaundryManagement.Application.DTOs.Orders;
using MediatR;

namespace LaundryManagement.Application.Commands.Payments;

public record CreatePaymentCommand(
    int OrderId,
    decimal Amount,
    int PaymentMethodId,
    string? Reference,
    string? Notes,
    string PaidAt,
    int ReceivedBy
) : IRequest<OrderPaymentDto>;
