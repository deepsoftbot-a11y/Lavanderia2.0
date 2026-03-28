namespace LaundryManagement.Domain.Common;

/// <summary>
/// Clase base abstracta para todos los Value Objects del dominio.
/// Los Value Objects se comparan por valor, no por identidad.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Obtiene los componentes atómicos que definen la igualdad del Value Object
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Compara este Value Object con otro objeto
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var valueObject = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(valueObject.GetEqualityComponents());
    }

    /// <summary>
    /// Compara este Value Object con otro Value Object
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    /// <summary>
    /// Obtiene el hash code basado en los componentes del Value Object
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Operador de igualdad
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Operador de desigualdad
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
