using LaundryManagement.Application.DTOs.ServiceGarments;
using MediatR;

namespace LaundryManagement.Application.Queries.ServiceGarments;

/// <summary>
/// Query para obtener una lista de tipos de prenda con filtros opcionales
/// </summary>
public sealed record GetServiceGarmentsQuery : IRequest<List<ServiceGarmentDto>>
{
    /// <summary>
    /// Filtro de búsqueda por nombre (opcional)
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro por estado activo (opcional: null = todos, true = solo activos, false = solo inactivos)
    /// </summary>
    public bool? IsActive { get; init; }
}
