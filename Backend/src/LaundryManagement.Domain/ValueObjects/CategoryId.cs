using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de una categoría de servicio.
/// </summary>
public sealed class CategoryId : ValueObject
{
    /// <summary>
    /// Valor del identificador
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private CategoryId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El CategoryId no puede ser negativo", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Crea un CategoryId desde un valor int
    /// </summary>
    public static CategoryId From(int value) => new CategoryId(value);

    /// <summary>
    /// Crea un CategoryId temporal
    /// </summary>
    public static CategoryId Empty() => new CategoryId(0);

    /// <summary>
    /// Verifica si el ID está vacío
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
    public static implicit operator int(CategoryId categoryId) => categoryId.Value;
}
