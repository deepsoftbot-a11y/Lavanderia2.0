using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado ClientPure.
/// Define el contrato para persistencia y recuperación de clientes.
/// </summary>
public interface IClientRepository
{
    #region Queries (Lectura)

    /// <summary>
    /// Obtiene un cliente por su ID
    /// </summary>
    /// <param name="clientId">ID del cliente</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Cliente o null si no se encuentra</returns>
    Task<ClientPure?> GetByIdAsync(ClientId clientId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un cliente por número telefónico
    /// </summary>
    /// <param name="phoneNumber">Número telefónico</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Cliente o null si no se encuentra</returns>
    Task<ClientPure?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los clientes
    /// </summary>
    /// <param name="includeInactive">Si true, incluye clientes inactivos</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Lista de clientes</returns>
    Task<IEnumerable<ClientPure>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Obtiene múltiples clientes por sus IDs (batch fetch para listados)
    /// </summary>
    Task<IEnumerable<ClientPure>> GetByIdsAsync(IEnumerable<ClientId> clientIds, CancellationToken ct = default);

    /// <summary>
    /// Verifica si existe un cliente con el número telefónico dado
    /// </summary>
    /// <param name="phoneNumber">Número telefónico a verificar</param>
    /// <param name="excludeClientId">ID de cliente a excluir de la búsqueda (útil para actualizaciones)</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>True si existe, False si no</returns>
    Task<bool> ExistsByPhoneNumberAsync(
        PhoneNumber phoneNumber,
        ClientId? excludeClientId = null,
        CancellationToken ct = default);

    #endregion

    #region Commands (Escritura)

    /// <summary>
    /// Agrega un nuevo cliente
    /// </summary>
    /// <param name="client">Cliente a agregar</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Cliente con ID asignado</returns>
    Task<ClientPure> AddAsync(ClientPure client, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    /// <param name="client">Cliente con cambios</param>
    /// <param name="ct">Token de cancelación</param>
    Task UpdateAsync(ClientPure client, CancellationToken ct = default);

    /// <summary>
    /// Persiste los cambios al contexto de base de datos
    /// </summary>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    #endregion
}
