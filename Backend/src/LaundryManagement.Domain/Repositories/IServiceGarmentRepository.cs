using LaundryManagement.Domain.Aggregates.ServiceGarments;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado ServiceGarmentPure.
/// Define el contrato para persistencia y recuperación de tipos de prenda.
/// La implementación estará en la capa de Infrastructure.
/// </summary>
public interface IServiceGarmentRepository
{
    #region Queries

    /// <summary>
    /// Obtiene un tipo de prenda por su identificador
    /// </summary>
    Task<ServiceGarmentPure?> GetByIdAsync(ServiceGarmentId garmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un tipo de prenda por su nombre
    /// </summary>
    Task<ServiceGarmentPure?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los tipos de prenda activos
    /// </summary>
    Task<IEnumerable<ServiceGarmentPure>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los tipos de prenda (activos e inactivos)
    /// </summary>
    Task<IEnumerable<ServiceGarmentPure>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene tipos de prenda por lista de ServicioPrendaIds (batch fetch para queries de órdenes).
    /// Retorna un diccionario keyed por ServicioPrendaId → ServiceGarmentPure (TiposPrenda).
    /// </summary>
    Task<Dictionary<int, ServiceGarmentPure>> GetGarmentTypesByServicioPrendaIdsAsync(IEnumerable<int> servicioPrendaIds, CancellationToken cancellationToken = default);

    #endregion

    #region Commands

    /// <summary>
    /// Agrega un nuevo tipo de prenda al repositorio
    /// </summary>
    Task<ServiceGarmentPure> AddAsync(ServiceGarmentPure garment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un tipo de prenda existente
    /// </summary>
    Task UpdateAsync(ServiceGarmentPure garment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un tipo de prenda (soft delete o hard delete según reglas de negocio)
    /// </summary>
    Task DeleteAsync(ServiceGarmentId garmentId, CancellationToken cancellationToken = default);

    #endregion

    #region Unit of Work

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos.
    /// Este método debe despachar los eventos de dominio antes de persistir.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    #endregion
}
