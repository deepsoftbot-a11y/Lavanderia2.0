using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Orders;

/// <summary>
/// Evento de dominio que se dispara cuando se entrega una orden
/// </summary>
public sealed class OrderDelivered : DomainEvent
{
    /// <summary>
    /// Identificador de la orden
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Fecha de entrega
    /// </summary>
    public DateTime FechaEntrega { get; }

    /// <summary>
    /// Identificador del usuario que entregó la orden
    /// </summary>
    public int EntregadoPor { get; }

    /// <summary>
    /// Constructor del evento OrderDelivered
    /// </summary>
    public OrderDelivered(int orderId, DateTime fechaEntrega, int entregadoPor)
    {
        OrderId = orderId;
        FechaEntrega = fechaEntrega;
        EntregadoPor = entregadoPor;
    }
}
