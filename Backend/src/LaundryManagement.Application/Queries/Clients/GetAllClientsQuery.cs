using LaundryManagement.Application.DTOs.Clients;
using MediatR;

namespace LaundryManagement.Application.Queries.Clients;

/// <summary>
/// Query para obtener todos los clientes con filtros opcionales
/// </summary>
public sealed record GetAllClientsQuery : IRequest<List<ClientDto>>
{
    /// <summary>
    /// Búsqueda en nombre, teléfono o número de cliente
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro por estado activo (null = todos, true = activos, false = inactivos)
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Campo por el cual ordenar: "name" o "createdAt"
    /// </summary>
    public string SortBy { get; init; } = "name";

    /// <summary>
    /// Dirección del ordenamiento: "asc" o "desc"
    /// </summary>
    public string SortOrder { get; init; } = "asc";
}
