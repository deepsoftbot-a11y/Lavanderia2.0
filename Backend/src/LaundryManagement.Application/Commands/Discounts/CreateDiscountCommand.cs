using LaundryManagement.Application.DTOs.Discounts;
using MediatR;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Comando para crear un nuevo descuento en el catálogo
/// </summary>
public sealed record CreateDiscountCommand : IRequest<DiscountDto>
{
    /// <summary>
    /// Nombre del descuento (debe ser único)
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Tipo de descuento: "None", "Percentage", "FixedAmount"
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Valor del descuento (0 para None, >0 para Percentage y FixedAmount)
    /// </summary>
    public decimal Value { get; init; }

    /// <summary>
    /// Monto mínimo de orden (ignorado — sin columna en BD actualmente)
    /// </summary>
    public decimal? MinOrderAmount { get; init; }

    /// <summary>
    /// Fecha de inicio de validez en formato "yyyy-MM-dd" (default: hoy si no se proporciona)
    /// </summary>
    public string? StartDate { get; init; }

    /// <summary>
    /// Fecha de fin de validez en formato "yyyy-MM-dd" (opcional)
    /// </summary>
    public string? EndDate { get; init; }
}
