namespace LaundryManagement.Domain.Common;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Define igualdad basada en el identificador único.
/// </summary>
/// <typeparam name="TId">Tipo del identificador de la entidad</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Compara esta entidad con otro objeto
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> entity)
            return false;

        if (ReferenceEquals(this, entity))
            return true;

        if (GetType() != entity.GetType())
            return false;

        return Id.Equals(entity.Id);
    }

    /// <summary>
    /// Compara esta entidad con otra entidad del mismo tipo
    /// </summary>
    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    /// <summary>
    /// Obtiene el hash code basado en el identificador
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Operador de igualdad
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
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
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
