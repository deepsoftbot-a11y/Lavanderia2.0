using LaundryManagement.Domain.Aggregates.Categories;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado CategoryPure.
/// Define el contrato para persistencia y recuperación de categorías.
/// La implementación estará en la capa de Infrastructure.
/// </summary>
public interface ICategoryRepository
{
    #region Queries

    /// <summary>
    /// Obtiene una categoría por su identificador
    /// </summary>
    Task<CategoryPure?> GetByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las categorías activas
    /// </summary>
    Task<IEnumerable<CategoryPure>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las categorías (activas e inactivas)
    /// </summary>
    Task<IEnumerable<CategoryPure>> GetAllAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Commands

    /// <summary>
    /// Agrega una nueva categoría al repositorio
    /// </summary>
    Task<CategoryPure> AddAsync(CategoryPure category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una categoría existente
    /// </summary>
    Task UpdateAsync(CategoryPure category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una categoría
    /// </summary>
    Task DeleteAsync(CategoryId categoryId, CancellationToken cancellationToken = default);

    #endregion

    #region Unit of Work

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    #endregion
}
