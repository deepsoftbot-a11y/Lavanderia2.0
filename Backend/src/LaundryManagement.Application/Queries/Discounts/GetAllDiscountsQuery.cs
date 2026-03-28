using LaundryManagement.Application.DTOs.Discounts;
using MediatR;

namespace LaundryManagement.Application.Queries.Discounts;

/// <summary>
/// Query para obtener todos los descuentos con filtros y ordenamiento opcionales.
/// Los nombres de propiedades coinciden con los query params que envía el frontend.
/// </summary>
public sealed record GetAllDiscountsQuery : IRequest<List<DiscountDto>>
{
    /// <summary>
    /// Búsqueda por nombre (case-insensitive contains)
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtro por tipo: NONE, PERCENTAGE, FIXED
    /// </summary>
    public string? Tipo { get; init; }

    /// <summary>
    /// Filtro por estado activo (null = todos)
    /// </summary>
    public bool? Activo { get; init; }

    /// <summary>
    /// Campo de ordenamiento: "name" | "value"
    /// </summary>
    public string OrdenarPor { get; init; } = "name";

    /// <summary>
    /// Dirección del ordenamiento: "asc" | "desc"
    /// </summary>
    public string Orden { get; init; } = "asc";
}
