using LaundryManagement.Domain.Aggregates.Services;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado ServicePure.
/// Define el contrato para persistencia y recuperación de servicios.
/// La implementación estará en la capa de Infrastructure.
/// </summary>
public interface IServiceRepository
{
    #region Queries

    /// <summary>
    /// Obtiene un servicio por su identificador
    /// </summary>
    Task<ServicePure?> GetByIdAsync(ServiceId serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un servicio por su código
    /// </summary>
    Task<ServicePure?> GetByCodeAsync(ServiceCode code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los servicios activos
    /// </summary>
    Task<IEnumerable<ServicePure>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los servicios (activos e inactivos)
    /// </summary>
    Task<IEnumerable<ServicePure>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene servicios por lista de IDs (batch fetch para queries de órdenes)
    /// </summary>
    Task<Dictionary<int, ServicePure>> GetByIdsAsync(IEnumerable<int> serviceIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el ServicioId y IsActive de un precio de servicio-prenda por su ID directamente (sin cargar servicios padre).
    /// Util para operaciones que solo necesitan modificar el precio sin hacer full table scan.
    /// </summary>
    Task<(int ServicioId, bool IsActive)?> GetServicePriceByIdAsync(int servicePriceId, CancellationToken cancellationToken = default);

    #endregion

    #region Commands

    /// <summary>
    /// Agrega un nuevo servicio al repositorio
    /// </summary>
    Task<ServicePure> AddAsync(ServicePure service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un servicio existente
    /// </summary>
    Task UpdateAsync(ServicePure service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un servicio (soft delete o hard delete según reglas de negocio)
    /// </summary>
    Task DeleteAsync(ServiceId serviceId, CancellationToken cancellationToken = default);

    #endregion

    #region Unit of Work

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos.
    /// Este método debe despachar los eventos de dominio antes de persistir.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    #endregion
}
