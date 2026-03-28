using LaundryManagement.Application.DTOs.Services;
using MediatR;

namespace LaundryManagement.Application.Queries.Services;

public sealed record GetServiceByIdQuery : IRequest<ServiceDto?>
{
    public int ServiceId { get; init; }
}
