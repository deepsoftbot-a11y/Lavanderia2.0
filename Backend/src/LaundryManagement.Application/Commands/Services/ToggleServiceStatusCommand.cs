using MediatR;

namespace LaundryManagement.Application.Commands.Services;

public sealed record ToggleServiceStatusCommand : IRequest<Unit>
{
    public int ServiceId { get; init; }
    public bool IsActive { get; init; }
}
