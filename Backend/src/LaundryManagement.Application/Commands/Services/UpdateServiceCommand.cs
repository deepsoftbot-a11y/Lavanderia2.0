using LaundryManagement.Application.DTOs.Services;
using MediatR;

namespace LaundryManagement.Application.Commands.Services;

public sealed record UpdateServiceCommand : IRequest<ServiceDto>
{
    public int ServiceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? PricePerKg { get; init; }
    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public string? Icon { get; init; }
    public decimal? EstimatedTime { get; init; }
    public bool? IsActive { get; init; }

    /// <summary>
    /// Lista de precios activos por prenda. Si se provee, se sincroniza como "verdad actual":
    /// activa los incluidos, desactiva los existentes que no están en la lista.
    /// Si es null, los precios no se modifican.
    /// </summary>
    public List<ServiceGarmentPriceInput>? GarmentPrices { get; init; }
}
