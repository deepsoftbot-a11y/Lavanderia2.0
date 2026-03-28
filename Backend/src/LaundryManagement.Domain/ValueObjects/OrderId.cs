using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de una orden.
/// Encapsula el ID como concepto de dominio, no solo un int primitivo.
/// </summary>
public sealed class OrderId : ValueObject
{
    /// <summary>
    /// Valor del identificador
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private OrderId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El OrderId no puede ser negativo", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Crea un OrderId desde un valor int
    /// </summary>
    public static OrderId From(int value) => new OrderId(value);

    /// <summary>
    /// Crea un OrderId temporal (para nuevas órdenes antes de persistir)
    /// </summary>
    public static OrderId Empty() => new OrderId(0);

    /// <summary>
    /// Verifica si el ID está vacío (orden no persistida aún)
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
    public static implicit operator int(OrderId orderId) => orderId.Value;
}
