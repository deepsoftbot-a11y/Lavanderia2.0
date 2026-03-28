using MediatR;

namespace LaundryManagement.Application.Commands.Services;

public sealed record AddServicePriceCommand : IRequest<int>
{
    public int ServiceId { get; init; }
    public int ServiceGarmentId { get; init; }
    public decimal Price { get; init; }
}
