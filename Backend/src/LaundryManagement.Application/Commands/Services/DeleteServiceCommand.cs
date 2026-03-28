using MediatR;

namespace LaundryManagement.Application.Commands.Services;

public sealed record DeleteServiceCommand : IRequest<Unit>
{
    public int ServiceId { get; init; }
}
