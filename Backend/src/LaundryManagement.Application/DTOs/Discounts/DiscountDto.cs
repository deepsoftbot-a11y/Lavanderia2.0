namespace LaundryManagement.Application.DTOs.Discounts;

/// <summary>
/// DTO que representa un descuento del catálogo.
/// Los nombres de propiedades coinciden con el contrato del frontend.
/// </summary>
public sealed record DiscountDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Tipo: "None", "Percentage", "FixedAmount"
    /// </summary>
    public string Type { get; init; } = string.Empty;

    public decimal Value { get; init; }

    public bool IsActive { get; init; }

    /// <summary>
    /// Monto mínimo de orden (no persistido actualmente, siempre null)
    /// </summary>
    public decimal? MinOrderAmount { get; init; }

    /// <summary>
    /// Fecha de inicio de validez en formato "yyyy-MM-dd"
    /// </summary>
    public string? StartDate { get; init; }

    /// <summary>
    /// Fecha de fin de validez en formato "yyyy-MM-dd" (o null)
    /// </summary>
    public string? EndDate { get; init; }

    /// <summary>
    /// Fecha de creación (derivada de StartDate como "yyyy-MM-dd'T'00:00:00")
    /// </summary>
    public string CreatedAt { get; init; } = string.Empty;

    /// <summary>
    /// Fecha de última actualización (no persistida actualmente, siempre null)
    /// </summary>
    public string? UpdatedAt { get; init; }
}
