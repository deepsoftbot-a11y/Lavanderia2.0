using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Orders;

/// <summary>
/// Evento de dominio que se dispara cuando se agrega un item a una orden
/// </summary>
public sealed class OrderLineItemAdded : DomainEvent
{
    /// <summary>
    /// Identificador de la orden
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Identificador del detalle de la orden (línea agregada)
    /// </summary>
    public int LineItemId { get; }

    /// <summary>
    /// Identificador del servicio
    /// </summary>
    public int ServicioId { get; }

    /// <summary>
    /// Peso en kilos (si aplica)
    /// </summary>
    public decimal? PesoKilos { get; }

    /// <summary>
    /// Cantidad de piezas (si aplica)
    /// </summary>
    public int? Cantidad { get; }

    /// <summary>
    /// Precio unitario
    /// </summary>
    public decimal PrecioUnitario { get; }

    /// <summary>
    /// Constructor del evento OrderLineItemAdded
    /// </summary>
    public OrderLineItemAdded(
        int orderId,
        int lineItemId,
        int servicioId,
        decimal? pesoKilos,
        int? cantidad,
        decimal precioUnitario)
    {
        OrderId = orderId;
        LineItemId = lineItemId;
        ServicioId = servicioId;
        PesoKilos = pesoKilos;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }
}
