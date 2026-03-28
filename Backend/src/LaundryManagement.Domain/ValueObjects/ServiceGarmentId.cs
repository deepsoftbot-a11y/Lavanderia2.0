using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de un tipo de prenda.
/// </summary>
public sealed class ServiceGarmentId : ValueObject
{
    /// <summary>
    /// Valor del identificador
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private ServiceGarmentId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El ServiceGarmentId no puede ser negativo", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Crea un ServiceGarmentId desde un valor int
    /// </summary>
    public static ServiceGarmentId From(int value) => new ServiceGarmentId(value);

    /// <summary>
    /// Crea un ServiceGarmentId temporal (para nuevos tipos de prenda antes de persistir)
    /// </summary>
    public static ServiceGarmentId Empty() => new ServiceGarmentId(0);

    /// <summary>
    /// Verifica si el ID está vacío (tipo de prenda no persistido aún)
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
    public static implicit operator int(ServiceGarmentId garmentId) => garmentId.Value;
}
