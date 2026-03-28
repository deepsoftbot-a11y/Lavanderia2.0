using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un descuento aplicado a una orden.
/// Entidad PURA de dominio.
/// </summary>
public sealed class OrderDiscount : Entity<int>
{
    /// <summary>
    /// Identificador del descuento (catálogo) - opcional
    /// </summary>
    public int? DiscountId { get; private set; }

    /// <summary>
    /// Identificador del combo - opcional
    /// </summary>
    public int? ComboId { get; private set; }

    /// <summary>
    /// Monto del descuento
    /// </summary>
    public Money DiscountAmount { get; private set; }

    /// <summary>
    /// Justificación del descuento
    /// </summary>
    public string? Justification { get; private set; }

    /// <summary>
    /// Usuario que aplicó el descuento
    /// </summary>
    public int AppliedBy { get; private set; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private OrderDiscount()
    {
        DiscountAmount = Money.Zero();
    }

    /// <summary>
    /// Crea un nuevo descuento de orden
    /// </summary>
    internal static OrderDiscount Create(
        int? discountId,
        int? comboId,
        Money discountAmount,
        int appliedBy,
        string? justification = null)
    {
        // Validaciones
        if (!discountId.HasValue && !comboId.HasValue)
            throw new BusinessRuleException("Debe especificar un descuento o un combo");

        if (discountAmount.IsZero || discountAmount.Amount <= 0)
            throw new ValidationException("El monto del descuento debe ser mayor a cero");

        if (appliedBy <= 0)
            throw new ValidationException("AppliedBy debe ser un usuario válido");

        return new OrderDiscount
        {
            DiscountId = discountId,
            ComboId = comboId,
            DiscountAmount = discountAmount,
            AppliedBy = appliedBy,
            Justification = justification
        };
    }

    /// <summary>
    /// Reconstituye un OrderDiscount desde la base de datos
    /// </summary>
    internal static OrderDiscount Reconstitute(
        int id,
        int? discountId,
        int? comboId,
        Money discountAmount,
        int appliedBy,
        string? justification)
    {
        return new OrderDiscount
        {
            Id = id,
            DiscountId = discountId,
            ComboId = comboId,
            DiscountAmount = discountAmount,
            AppliedBy = appliedBy,
            Justification = justification
        };
    }
}
