using LaundryManagement.Application.DTOs.ServicePrices;
using MediatR;

namespace LaundryManagement.Application.Queries.ServicePrices;

/// <summary>
/// Query para obtener precios de servicio-prenda con filtros opcionales
/// </summary>
public sealed record GetServicePricesQuery : IRequest<List<ServicePriceDto>>
{
    /// <summary>
    /// Filtro por ID de servicio (opcional)
    /// </summary>
    public int? ServiceId { get; init; }

    /// <summary>
    /// Filtro por ID de tipo de prenda (opcional)
    /// </summary>
    public int? ServiceGarmentId { get; init; }

    /// <summary>
    /// Filtro por estado activo (opcional: null = todos, true = solo activos, false = solo inactivos)
    /// </summary>
    public bool? IsActive { get; init; }
}
