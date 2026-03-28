using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de un descuento.
/// </summary>
public sealed class DiscountId : ValueObject
{
    public int Value { get; }

    private DiscountId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El DiscountId no puede ser negativo", nameof(value));

        Value = value;
    }

    public static DiscountId From(int value) => new(value);

    /// <summary>
    /// Crea un DiscountId temporal (para descuentos antes de persistir)
    /// </summary>
    public static DiscountId Empty() => new(0);

    /// <summary>
    /// Verifica si el ID está vacío (descuento no persistido aún)
    /// </summary>
    public bool IsEmpty => Value == 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(DiscountId discountId) => discountId.Value;
}
