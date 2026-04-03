using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio para el agregado Order PURO.
/// Define el contrato para persistencia y recuperación de órdenes.
/// La implementación estará en la capa de Infrastructure.
/// </summary>
public interface IOrderRepository
{
    #region Queries

    /// <summary>
    /// Obtiene una orden por su identificador
    /// </summary>
    /// <param name="orderId">Identificador de la orden</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Orden encontrada o null si no existe</returns>
    Task<OrderPure?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una orden por su folio
    /// </summary>
    /// <param name="folio">Folio de la orden</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Orden encontrada o null si no existe</returns>
    Task<OrderPure?> GetByFolioAsync(OrderFolio folio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las órdenes de un cliente
    /// </summary>
    Task<IEnumerable<OrderPure>> GetByClientAsync(ClientId clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las órdenes aplicando filtros opcionales con soporte de paginación.
    /// </summary>
    /// <param name="search">Busca en folio, nombre completo o teléfono del cliente</param>
    /// <param name="clientId">Filtro por cliente</param>
    /// <param name="startDate">Fecha de inicio (recepción)</param>
    /// <param name="endDate">Fecha de fin (recepción)</param>
    /// <param name="statusIds">Filtro por uno o varios IDs de estado de orden</param>
    /// <param name="paymentStatuses">Filtro por estado de pago: "paid", "partial", "pending"</param>
    /// <param name="sortBy">Campo de ordenamiento</param>
    /// <param name="sortOrder">Dirección: "asc" o "desc"</param>
    /// <param name="page">Número de página (1-based)</param>
    /// <param name="pageSize">Tamaño de página; int.MaxValue devuelve todo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tupla con los ítems de la página y el total de registros sin paginar</returns>
    Task<(IEnumerable<OrderPure> Items, int TotalCount)> GetAllAsync(
        string? search = null,
        int? clientId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int[]? statusIds = null,
        string[]? paymentStatuses = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = int.MaxValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera el siguiente folio secuencial para una orden.
    /// Este método debe llamarse ANTES de iniciar transacciones para evitar conflictos
    /// con EnableRetryOnFailure.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Folio generado en formato ORD-YYYYMMDD-NNNN</returns>
    Task<string> GenerateNextFolioAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Commands

    /// <summary>
    /// Agrega una nueva orden al repositorio
    /// </summary>
    /// <param name="order">Orden a agregar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Orden agregada con su ID asignado</returns>
    Task<OrderPure> AddAsync(OrderPure order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza las propiedades escalares de una orden existente (estado, fechas, totales, notas, etc.).
    /// NO modifica las colecciones hijas (detalles ni descuentos).
    /// </summary>
    Task UpdateAsync(OrderPure order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reemplaza completamente los line items y descuentos de una orden.
    /// Usar solo cuando los items realmente cambiaron (ej. UpdateOrderCommand).
    /// </summary>
    Task ReplaceLineItemsAsync(OrderPure order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una orden (usado para escenarios de rollback)
    /// </summary>
    /// <param name="orderId">ID de la orden a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task DeleteAsync(OrderId orderId, CancellationToken cancellationToken = default);

    #endregion

    #region Unit of Work

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos.
    /// Este método debe despachar los eventos de dominio antes de persistir.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros afectados</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una orden con pago opcional en una única transacción atómica.
    /// Este método se usa cuando el pago inicial es inseparable de la creación de la orden,
    /// garantizando que ambas operaciones se ejecuten en la misma transacción usando ExecutionStrategy.
    /// </summary>
    /// <param name="order">Orden a crear (sin folio asignado)</param>
    /// <param name="preGeneratedFolio">Folio pre-generado (debe generarse ANTES de llamar este método)</param>
    /// <param name="paymentData">Datos del pago inicial opcional</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tupla con la orden creada y el ID del pago (si se proporcionó)</returns>
    Task<(OrderPure Order, int? PaymentId)> CreateOrderWithPaymentAsync(
        OrderPure order,
        string preGeneratedFolio,
        InitialPaymentData? paymentData,
        CancellationToken cancellationToken = default
    );

    #endregion
}
