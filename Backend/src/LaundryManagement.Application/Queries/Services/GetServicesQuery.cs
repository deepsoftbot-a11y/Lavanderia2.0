using LaundryManagement.Application.DTOs.Services;
using MediatR;

namespace LaundryManagement.Application.Queries.Services;

public sealed record GetServicesQuery : IRequest<List<ServiceDto>>
{
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public string? ChargeType { get; init; }
    public bool? IsActive { get; init; }
}
