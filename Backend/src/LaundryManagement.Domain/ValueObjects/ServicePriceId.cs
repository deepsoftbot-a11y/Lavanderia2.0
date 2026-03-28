using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de un precio de servicio-prenda.
/// </summary>
public sealed class ServicePriceId : ValueObject
{
    /// <summary>
    /// Valor del identificador
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private ServicePriceId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El ServicePriceId no puede ser negativo", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Crea un ServicePriceId desde un valor int
    /// </summary>
    public static ServicePriceId From(int value) => new ServicePriceId(value);

    /// <summary>
    /// Crea un ServicePriceId temporal (para nuevos precios antes de persistir)
    /// </summary>
    public static ServicePriceId Empty() => new ServicePriceId(0);

    /// <summary>
    /// Verifica si el ID está vacío (precio no persistido aún)
    /// </summary>
    public bool IsEmpty => Value == 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Conversión implícita a int
    /// </summary>
    public static implicit operator int(ServicePriceId priceId) => priceId.Value;
}
