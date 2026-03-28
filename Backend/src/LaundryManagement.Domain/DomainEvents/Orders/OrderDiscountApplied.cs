using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Orders;

/// <summary>
/// Evento de dominio que se dispara cuando se aplica un descuento a una orden
/// </summary>
public sealed class OrderDiscountApplied : DomainEvent
{
    /// <summary>
    /// Identificador de la orden
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// Identificador del descuento (si aplica)
    /// </summary>
    public int? DescuentoId { get; }

    /// <summary>
    /// Identificador del combo (si aplica)
    /// </summary>
    public int? ComboId { get; }

    /// <summary>
    /// Monto del descuento aplicado
    /// </summary>
    public decimal MontoDescuento { get; }

    /// <summary>
    /// Identificador del usuario que aplicó el descuento
    /// </summary>
    public int AplicadoPor { get; }

    /// <summary>
    /// Constructor del evento OrderDiscountApplied
    /// </summary>
    public OrderDiscountApplied(
        int orderId,
        int? descuentoId,
        int? comboId,
        decimal montoDescuento,
        int aplicadoPor)
    {
        OrderId = orderId;
        DescuentoId = descuentoId;
        ComboId = comboId;
        MontoDescuento = montoDescuento;
        AplicadoPor = aplicadoPor;
    }
}
