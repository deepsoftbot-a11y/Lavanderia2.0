using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Categories;

/// <summary>
/// Agregado CategoryPure - Entidad de dominio que representa una categoría de servicios.
/// </summary>
public sealed class CategoryPure : AggregateRoot<CategoryId>
{
    #region Propiedades de Dominio

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indica si la categoría está activa
    /// </summary>
    public bool IsActive { get; private set; }

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado para reconstitución desde BD
    /// </summary>
    private CategoryPure()
    {
        Name = string.Empty;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea una nueva categoría
    /// </summary>
    public static CategoryPure Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre de la categoría es requerido");

        return new CategoryPure
        {
            Id = CategoryId.Empty(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    /// <summary>
    /// Reconstituye una categoría desde la base de datos (usado por Repository)
    /// </summary>
    internal static CategoryPure Reconstitute(
        CategoryId id,
        string name,
        string? description,
        bool isActive)
    {
        return new CategoryPure
        {
            Id = id,
            Name = name,
            Description = description,
            IsActive = isActive
        };
    }

    #endregion

    #region Métodos de Dominio

    /// <summary>
    /// Actualiza la información de la categoría
    /// </summary>
    public void UpdateInfo(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre de la categoría es requerido");

        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>
    /// Activa la categoría
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Desactiva la categoría
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Establece el ID (usado por el repository después de persistir)
    /// </summary>
    internal void SetId(CategoryId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido establecido");

        Id = id;
    }

    #endregion
}
