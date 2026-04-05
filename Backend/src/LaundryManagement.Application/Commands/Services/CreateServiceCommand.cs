using LaundryManagement.Application.DTOs.Services;
using MediatR;

namespace LaundryManagement.Application.Commands.Services;

/// <summary>
/// Command para crear un nuevo servicio usando DDD
/// </summary>
public sealed record CreateServiceCommand : IRequest<ServiceDto>
{
    public string? Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CategoryId { get; init; }
    public string ChargeType { get; init; } = string.Empty; // "piece" o "kg"

    /// <summary>
    /// Precio por unidad (para piezas) o por kilo (para servicios por peso)
    /// </summary>
    public decimal? PricePerKg { get; init; }

    public decimal? MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }

    public string? Icon { get; init; }
    public decimal? EstimatedTime { get; init; }
    public List<ServiceGarmentPriceInput>? GarmentPrices { get; init; }
}

public sealed record ServiceGarmentPriceInput
{
    public int GarmentTypeId { get; init; }
    public decimal UnitPrice { get; init; }
}
