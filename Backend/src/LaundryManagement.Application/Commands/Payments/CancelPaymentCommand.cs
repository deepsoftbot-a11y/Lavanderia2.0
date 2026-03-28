using MediatR;

namespace LaundryManagement.Application.Commands.Payments;

public record CancelPaymentCommand(int PaymentId, int CancelledBy) : IRequest;
