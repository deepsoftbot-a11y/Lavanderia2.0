using LaundryManagement.Application.DTOs.Discounts;
using MediatR;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Comando para actualizar un descuento existente.
/// Los campos opcionales que no se envían se mantienen con su valor actual.
/// </summary>
public sealed record UpdateDiscountCommand : IRequest<DiscountDto>
{
    /// <summary>
    /// ID del descuento a actualizar (se asigna desde la URL en el controller)
    /// </summary>
    public int DiscountId { get; init; }

    public string? Name { get; init; }

    /// <summary>
    /// Tipo de descuento: "None", "Percentage", "FixedAmount"
    /// </summary>
    public string? Type { get; init; }

    public decimal? Value { get; init; }

    /// <summary>
    /// Ignorado — sin columna en BD actualmente
    /// </summary>
    public decimal? MinOrderAmount { get; init; }

    /// <summary>
    /// Fecha de inicio en formato "yyyy-MM-dd". Null = mantener actual.
    /// </summary>
    public string? StartDate { get; init; }

    /// <summary>
    /// Fecha de fin en formato "yyyy-MM-dd". Null = mantener actual. "" = limpiar.
    /// </summary>
    public string? EndDate { get; init; }

    /// <summary>
    /// Ignorado — usar PATCH /status para cambiar estado activo
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Ignorado — no persistido actualmente
    /// </summary>
    public string? UpdatedAt { get; init; }
}
