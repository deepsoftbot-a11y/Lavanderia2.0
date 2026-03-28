using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de un servicio.
/// </summary>
public sealed class ServiceId : ValueObject
{
    /// <summary>
    /// Valor del identificador
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private ServiceId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El ServiceId no puede ser negativo", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Crea un ServiceId desde un valor int
    /// </summary>
    public static ServiceId From(int value) => new ServiceId(value);

    /// <summary>
    /// Crea un ServiceId temporal (para nuevos servicios antes de persistir)
    /// </summary>
    public static ServiceId Empty() => new ServiceId(0);

    /// <summary>
    /// Verifica si el ID está vacío (servicio no persistido aún)
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
    public static implicit operator int(ServiceId serviceId) => serviceId.Value;
}
