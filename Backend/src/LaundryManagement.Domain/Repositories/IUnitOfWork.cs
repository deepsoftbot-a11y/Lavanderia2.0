using System.Data;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Patrón Unit of Work para coordinar operaciones transaccionales
/// a través de múltiples agregados y servicios.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Inicia una nueva transacción explícita.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual, persistiendo todos los cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual, descartando todos los cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda todos los cambios pendientes dentro de la transacción actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros afectados</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la conexión de base de datos subyacente para operaciones Dapper.
    /// </summary>
    /// <returns>Conexión de base de datos</returns>
    IDbConnection GetConnection();

    /// <summary>
    /// Obtiene la transacción actual para operaciones Dapper que necesitan
    /// ejecutarse dentro de la misma transacción.
    /// </summary>
    /// <returns>Transacción actual o null si no hay transacción activa</returns>
    IDbTransaction? GetTransaction();
}
