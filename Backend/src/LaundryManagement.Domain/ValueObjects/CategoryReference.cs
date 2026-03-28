using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una referencia a una categoría.
/// Encapsula el ID y nombre de la categoría para uso en otros agregados.
/// </summary>
public sealed class CategoryReference : ValueObject
{
    /// <summary>
    /// Identificador de la categoría
    /// </summary>
    public CategoryId Id { get; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private CategoryReference(CategoryId id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Crea una referencia a categoría
    /// </summary>
    public static CategoryReference Create(CategoryId id, string name)
    {
        if (id.IsEmpty)
            throw new ValidationException("El ID de categoría es requerido");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre de categoría es requerido");

        return new CategoryReference(id, name.Trim());
    }

    /// <summary>
    /// Crea una referencia a categoría permitiendo nombre nulo (para reconstitución)
    /// </summary>
    internal static CategoryReference Reconstitute(CategoryId id, string? name)
    {
        return new CategoryReference(id, name ?? "Sin categoría");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Name;
    }

    public override string ToString() => $"{Name} ({Id.Value})";
}
