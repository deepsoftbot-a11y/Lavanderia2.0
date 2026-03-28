using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Orders;

/// <summary>
/// Evento de dominio que se dispara cuando se recalcula el total de una orden
/// </summary>
public sealed class OrderTotalRecalculated : DomainEvent
{
    /// <summary>
    /// Identificador de la orden
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Total anterior
    /// </summary>
    public decimal PreviousTotal { get; }

    /// <summary>
    /// Nuevo total
    /// </summary>
    public decimal NewTotal { get; }

    /// <summary>
    /// Constructor del evento OrderTotalRecalculated
    /// </summary>
    public OrderTotalRecalculated(int orderId, decimal previousTotal, decimal newTotal)
    {
        OrderId = orderId;
        PreviousTotal = previousTotal;
        NewTotal = newTotal;
    }
}
