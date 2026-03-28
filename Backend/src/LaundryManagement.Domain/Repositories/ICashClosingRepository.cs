using LaundryManagement.Domain.Aggregates.CashClosings;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado CashClosing PURO.
/// Define el contrato para persistencia y recuperación de cortes de caja.
/// La implementación estará en la capa de Infrastructure.
/// </summary>
public interface ICashClosingRepository
{
    #region Queries

    /// <summary>
    /// Obtiene un corte de caja por su identificador
    /// </summary>
    /// <param name="id">Identificador del corte</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Corte encontrado o null si no existe</returns>
    Task<CashClosingPure?> GetByIdAsync(CashClosingId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un corte de caja por su folio
    /// </summary>
    /// <param name="folio">Folio del corte</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Corte encontrado o null si no existe</returns>
    Task<CashClosingPure?> GetByFolioAsync(CashClosingFolio folio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los cortes de caja de un cajero en un rango de fechas
    /// </summary>
    /// <param name="cashierId">Identificador del cajero</param>
    /// <param name="startDate">Fecha inicial</param>
    /// <param name="endDate">Fecha final</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de cortes del cajero</returns>
    Task<IEnumerable<CashClosingPure>> GetByCashierAndDateRangeAsync(
        int cashierId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los totales de pagos de un día específico para un cajero.
    /// Consulta optimizada para obtener totales por método de pago.
    /// </summary>
    /// <param name="fecha">Fecha del día a consultar</param>
    /// <param name="cashierId">Identificador del cajero</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Totales por método de pago</returns>
    Task<DayPaymentTotals> GetDayTotalsAsync(DateTime fecha, int cashierId, CancellationToken cancellationToken = default);

    #endregion

    #region Commands

    /// <summary>
    /// Agrega un nuevo corte de caja al repositorio
    /// </summary>
    /// <param name="cashClosing">Corte a agregar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Corte agregado con su ID asignado</returns>
    Task<CashClosingPure> AddAsync(CashClosingPure cashClosing, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un corte de caja existente
    /// </summary>
    /// <param name="cashClosing">Corte a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task UpdateAsync(CashClosingPure cashClosing, CancellationToken cancellationToken = default);

    #endregion

    #region Unit of Work

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos.
    /// Este método debe despachar los eventos de dominio antes de persistir.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros afectados</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    #endregion
}
