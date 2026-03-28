using MediatR;
using LaundryManagement.Application.Queries.Orders;

namespace LaundryManagement.Application.Commands.Orders;

public record UpdateOrderStatusCommand(int OrderId, int NewStatusId, int ChangedBy = 1)
    : IRequest<OrderResponseDto>;
