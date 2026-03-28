using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Orders;

/// <summary>
/// Evento de dominio que se dispara cuando cambia el estado de una orden
/// </summary>
public sealed class OrderStatusChanged : DomainEvent
{
    /// <summary>
    /// Identificador de la orden
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Identificador del estado anterior
    /// </summary>
    public int PreviousStatusId { get; }

    /// <summary>
    /// Identificador del nuevo estado
    /// </summary>
    public int NewStatusId { get; }

    /// <summary>
    /// Identificador del usuario que cambió el estado
    /// </summary>
    public int ChangedBy { get; }

    /// <summary>
    /// Fecha del cambio de estado
    /// </summary>
    public DateTime ChangeDate { get; }

    /// <summary>
    /// Constructor del evento OrderStatusChanged
    /// </summary>
    public OrderStatusChanged(
        int orderId,
        int previousStatusId,
        int newStatusId,
        int changedBy,
        DateTime changeDate)
    {
        OrderId = orderId;
        PreviousStatusId = previousStatusId;
        NewStatusId = newStatusId;
        ChangedBy = changedBy;
        ChangeDate = changeDate;
    }
}
