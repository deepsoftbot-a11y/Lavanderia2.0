using MediatR;

namespace LaundryManagement.Application.Commands.Orders;

public record CancelOrderCommand(int OrderId, int CancelledBy) : IRequest;
