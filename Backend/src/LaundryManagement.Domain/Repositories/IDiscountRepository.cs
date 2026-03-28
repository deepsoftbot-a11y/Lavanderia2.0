using LaundryManagement.Domain.Aggregates.Discounts;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado DiscountPure.
/// Definida en Domain; implementada en Infrastructure.
/// </summary>
public interface IDiscountRepository
{
    #region Queries (Lectura)

    /// <summary>
    /// Obtiene un descuento por su ID
    /// </summary>
    Task<DiscountPure?> GetByIdAsync(DiscountId id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los descuentos del catálogo
    /// </summary>
    Task<IEnumerable<DiscountPure>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un descuento con el nombre dado.
    /// excludeId permite ignorar un ID específico (útil en actualizaciones).
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, DiscountId? excludeId = null, CancellationToken ct = default);

    #endregion

    #region Commands (Escritura)

    /// <summary>
    /// Agrega un nuevo descuento y retorna el agregado con ID asignado
    /// </summary>
    Task<DiscountPure> AddAsync(DiscountPure discount, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un descuento existente
    /// </summary>
    Task UpdateAsync(DiscountPure discount, CancellationToken ct = default);

    /// <summary>
    /// Elimina un descuento por su ID (hard delete)
    /// </summary>
    Task DeleteAsync(DiscountId id, CancellationToken ct = default);

    /// <summary>
    /// Persiste los cambios pendientes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    #endregion
}
