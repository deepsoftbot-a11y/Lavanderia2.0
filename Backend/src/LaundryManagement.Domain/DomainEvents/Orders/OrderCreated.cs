using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Orders;

/// <summary>
/// Evento de dominio que se dispara cuando se crea una nueva orden
/// </summary>
public sealed class OrderCreated : DomainEvent
{
    /// <summary>
    /// Identificador de la orden creada
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Identificador del cliente
    /// </summary>
    public int ClienteId { get; }

    /// <summary>
    /// Identificador del usuario que recibió la orden
    /// </summary>
    public int RecibidoPor { get; }

    /// <summary>
    /// Fecha de recepción de la orden
    /// </summary>
    public DateTime FechaRecepcion { get; }

    /// <summary>
    /// Constructor del evento OrderCreated
    /// </summary>
    public OrderCreated(int orderId, int clienteId, int recibidoPor, DateTime fechaRecepcion)
    {
        OrderId = orderId;
        ClienteId = clienteId;
        RecibidoPor = recibidoPor;
        FechaRecepcion = fechaRecepcion;
    }
}
