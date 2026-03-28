using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Discounts;

/// <summary>
/// Agregado Discount PURO - Entidad de dominio rica completamente independiente de infraestructura.
/// Representa un descuento del catálogo de la lavandería.
/// </summary>
public sealed class DiscountPure : AggregateRoot<DiscountId>
{
    #region Propiedades de Dominio

    public new DiscountId Id { get; private set; } = null!;

    /// <summary>
    /// Nombre del descuento (único)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Tipo de descuento: NONE, PERCENTAGE, FIXED
    /// </summary>
    public DiscountType Type { get; private set; }

    /// <summary>
    /// Valor del descuento (0 para NONE, >0 para PERCENTAGE y FIXED)
    /// </summary>
    public Money Value { get; private set; }

    /// <summary>
    /// Indica si el descuento está activo
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de inicio de validez del descuento
    /// </summary>
    public DateOnly ValidFrom { get; private set; }

    /// <summary>
    /// Fecha de fin de validez del descuento (opcional)
    /// </summary>
    public DateOnly? ValidUntil { get; private set; }

    #endregion

    #region Constructores

    private DiscountPure()
    {
        Name  = string.Empty;
        Type  = DiscountType.None();
        Value = Money.Zero();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea un nuevo descuento con validación de reglas de negocio
    /// </summary>
    public static DiscountPure Create(
        string name,
        DiscountType type,
        Money value,
        DateOnly validFrom,
        DateOnly? validUntil = null)
    {
        ValidateName(name);
        ValidateTypeValueConsistency(type, value);
        ValidateDateRange(validFrom, validUntil);

        return new DiscountPure
        {
            Id         = DiscountId.Empty(),
            Name       = name.Trim(),
            Type       = type,
            Value      = value,
            IsActive   = true,
            ValidFrom  = validFrom,
            ValidUntil = validUntil
        };
    }

    /// <summary>
    /// Reconstituye un descuento desde la base de datos (usado por Repository).
    /// INTERNAL - Solo accesible desde Infrastructure via InternalsVisibleTo.
    /// </summary>
    internal static DiscountPure Reconstitute(
        DiscountId id,
        string name,
        DiscountType type,
        Money value,
        bool isActive,
        DateOnly validFrom,
        DateOnly? validUntil)
    {
        return new DiscountPure
        {
            Id         = id,
            Name       = name,
            Type       = type,
            Value      = value,
            IsActive   = isActive,
            ValidFrom  = validFrom,
            ValidUntil = validUntil
        };
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Actualiza la información del descuento.
    /// Todos los parámetros representan el estado final deseado.
    /// El handler debe calcular los valores efectivos antes de llamar.
    /// </summary>
    public void UpdateInfo(
        string name,
        DiscountType type,
        Money value,
        DateOnly validFrom,
        DateOnly? validUntil)
    {
        ValidateName(name);
        ValidateTypeValueConsistency(type, value);
        ValidateDateRange(validFrom, validUntil);

        Name       = name.Trim();
        Type       = type;
        Value      = value;
        ValidFrom  = validFrom;
        ValidUntil = validUntil;
    }

    /// <summary>
    /// Activa el descuento
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new BusinessRuleException("El descuento ya está activo");

        IsActive = true;
    }

    /// <summary>
    /// Desactiva el descuento.
    /// El descuento de tipo NONE ("Sin descuento") no puede ser desactivado.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleException("El descuento ya está inactivo");

        if (Type.IsNone)
            throw new ConflictException("No se puede desactivar el descuento 'Sin descuento'");

        IsActive = false;
    }

    /// <summary>
    /// Asigna el ID después de la persistencia (usado por repository)
    /// </summary>
    internal void SetId(DiscountId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido asignado");

        Id = id;
    }

    #endregion

    #region Private Validation Methods

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del descuento no puede estar vacío");

        if (name.Trim().Length < 2)
            throw new ValidationException("El nombre debe tener al menos 2 caracteres");

        if (name.Length > 100)
            throw new ValidationException("El nombre no puede exceder 100 caracteres");
    }

    private static void ValidateTypeValueConsistency(DiscountType type, Money value)
    {
        if (type.IsNone && !value.IsZero)
            throw new ValidationException("El descuento de tipo NONE debe tener valor 0");

        if ((type.IsPercentage || type.IsFixed) && value.IsZero)
            throw new ValidationException(
                "Los descuentos de tipo PERCENTAGE o FIXED deben tener un valor mayor que 0");
    }

    private static void ValidateDateRange(DateOnly validFrom, DateOnly? validUntil)
    {
        if (validUntil.HasValue && validUntil.Value < validFrom)
            throw new ValidationException("La fecha de fin debe ser mayor o igual a la fecha de inicio");
    }

    #endregion
}
