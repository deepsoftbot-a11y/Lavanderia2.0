using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el tipo de descuento.
/// NONE = Sin descuento (especial, no se puede eliminar ni desactivar)
/// PERCENTAGE = Descuento porcentual
/// FIXED = Descuento de monto fijo
/// </summary>
public sealed class DiscountType : ValueObject
{
    public enum Type
    {
        Ninguno,
        Porcentaje,
        MontoFijo
    }

    public Type Value { get; }

    private DiscountType(Type value)
    {
        Value = value;
    }

    public static DiscountType None()       => new(Type.Ninguno);
    public static DiscountType Percentage() => new(Type.Porcentaje);
    public static DiscountType Fixed()      => new(Type.MontoFijo);

    /// <summary>
    /// Crea un DiscountType desde un string (case-insensitive).
    /// Acepta: "NONE", "PERCENTAGE", "FIXED"
    /// </summary>
    public static DiscountType From(string typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            throw new ValidationException("El tipo de descuento no puede estar vacío");

        return typeString.Trim().ToUpperInvariant() switch
        {
            "NINGUNO"   => None(),
            "PORCENTAJE" => Percentage(),
            "MONTOFIJO" => Fixed(),
            _ => throw new ValidationException(
                $"Tipo de descuento inválido: {typeString}. Valores válidos: Ninguno, Porcentaje, MontoFijo")
        };
    }

    public bool IsNone       => Value == Type.Ninguno;
    public bool IsPercentage => Value == Type.Porcentaje;
    public bool IsFixed      => Value == Type.MontoFijo;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
