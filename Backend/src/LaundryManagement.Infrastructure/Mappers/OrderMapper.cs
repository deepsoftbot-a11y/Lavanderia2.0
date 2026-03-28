using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper que traduce entre entidades de dominio (OrderPure) y entidades de infraestructura (Ordene).
/// Esta es la capa de anti-corrupción que mantiene el dominio puro independiente de la base de datos.
/// </summary>
public static class OrderMapper
{
    /// <summary>
    /// Mapea de entidad de dominio a entidad de infraestructura
    /// </summary>
    public static Ordene ToInfrastructure(OrderPure order)
    {
        var ordenEntity = new Ordene
        {
            // NO asignar OrdenId si es 0 (nueva orden) - dejar que SQL Server genere el IDENTITY
            FolioOrden = order.Folio.Value,
            ClienteId = order.ClientId.Value,
            FechaRecepcion = order.ReceivedDate,
            FechaPrometida = order.PromisedDate,
            FechaEntrega = order.DeliveryDate,
            EstadoOrdenId = order.StatusId,
            Subtotal = order.Subtotal.Amount,
            Descuento = order.TotalDiscount.Amount,
            Total = order.Total.Amount,
            Observaciones = order.Notes,
            Ubicaciones = order.StorageLocation,
            RecibidoPor = order.ReceivedBy,
            EntregadoPor = order.DeliveredBy,
            OrdenesDetalles = order.LineItems.Select(ToInfrastructure).ToList(),
            OrdenesDescuentos = order.Discounts.Select(ToInfrastructure).ToList(),
            HistorialEstadosOrdens = new List<HistorialEstadosOrden>() // Se gestiona por separado
        };

        // Solo asignar OrdenId si ya existe (no es una nueva orden)
        if (!order.Id.IsEmpty)
        {
            ordenEntity.OrdenId = order.Id.Value;
        }

        return ordenEntity;
    }

    /// <summary>
    /// Mapea de entidad de infraestructura a entidad de dominio
    /// </summary>
    public static OrderPure ToDomain(Ordene ordenEntity)
    {
        var lineItems = ordenEntity.OrdenesDetalles
            .Select(ToDomain)
            .ToList();

        var discounts = ordenEntity.OrdenesDescuentos
            .Select(ToDomain)
            .ToList();

        return OrderPure.Reconstitute(
            id: OrderId.From(ordenEntity.OrdenId),
            folio: OrderFolio.FromString(ordenEntity.FolioOrden),
            clientId: ClientId.From(ordenEntity.ClienteId),
            receivedDate: ordenEntity.FechaRecepcion,
            promisedDate: ordenEntity.FechaPrometida,
            deliveryDate: ordenEntity.FechaEntrega,
            statusId: ordenEntity.EstadoOrdenId,
            subtotal: Money.FromDecimal(ordenEntity.Subtotal),
            totalDiscount: Money.FromDecimal(ordenEntity.Descuento),
            total: Money.FromDecimal(ordenEntity.Total),
            notes: ordenEntity.Observaciones,
            storageLocation: ordenEntity.Ubicaciones,
            receivedBy: ordenEntity.RecibidoPor,
            deliveredBy: ordenEntity.EntregadoPor,
            lineItems: lineItems,
            discounts: discounts
        );
    }

    /// <summary>
    /// Mapea OrderLineItem de dominio a OrdenesDetalle de infraestructura
    /// </summary>
    private static OrdenesDetalle ToInfrastructure(OrderLineItem lineItem)
    {
        var detalle = new OrdenesDetalle
        {
            // NO asignar OrdenDetalleId ni OrdenId - EF Core los manejará automáticamente
            NumeroLinea = lineItem.LineNumber,
            ServicioId = lineItem.ServiceId,
            ServicioPrendaId = lineItem.ServiceGarmentId,
            PesoKilos = lineItem.WeightKilos,
            Cantidad = lineItem.Quantity,
            PrecioUnitario = lineItem.UnitPrice.Amount,
            Subtotal = lineItem.Subtotal.Amount,
            DescuentoLinea = lineItem.LineDiscount.Amount,
            TotalLinea = lineItem.LineTotal.Amount,
            Observaciones = lineItem.Notes
        };

        // Solo asignar IDs si ya existen (para actualizaciones)
        if (lineItem.Id > 0)
        {
            detalle.OrdenDetalleId = lineItem.Id;
        }

        return detalle;
    }

    /// <summary>
    /// Mapea OrdenesDetalle de infraestructura a OrderLineItem de dominio
    /// </summary>
    private static OrderLineItem ToDomain(OrdenesDetalle detalleEntity)
    {
        return OrderLineItem.Reconstitute(
            id: detalleEntity.OrdenDetalleId,
            lineNumber: detalleEntity.NumeroLinea,
            serviceId: detalleEntity.ServicioId,
            serviceGarmentId: detalleEntity.ServicioPrendaId,
            weightKilos: detalleEntity.PesoKilos,
            quantity: detalleEntity.Cantidad,
            unitPrice: Money.FromDecimal(detalleEntity.PrecioUnitario),
            subtotal: Money.FromDecimal(detalleEntity.Subtotal),
            lineDiscount: Money.FromDecimal(detalleEntity.DescuentoLinea),
            lineTotal: Money.FromDecimal(detalleEntity.TotalLinea),
            notes: detalleEntity.Observaciones
        );
    }

    /// <summary>
    /// Mapea OrderDiscount de dominio a OrdenesDescuento de infraestructura
    /// </summary>
    private static OrdenesDescuento ToInfrastructure(OrderDiscount discount)
    {
        var descuento = new OrdenesDescuento
        {
            // NO asignar OrdenDescuentoId ni OrdenId - EF Core los manejará automáticamente
            DescuentoId = discount.DiscountId,
            ComboId = discount.ComboId,
            MontoDescuento = discount.DiscountAmount.Amount,
            Justificacion = discount.Justification,
            AplicadoPor = discount.AppliedBy
        };

        // Solo asignar IDs si ya existen (para actualizaciones)
        if (discount.Id > 0)
        {
            descuento.OrdenDescuentoId = discount.Id;
        }

        return descuento;
    }

    /// <summary>
    /// Mapea OrdenesDescuento de infraestructura a OrderDiscount de dominio
    /// </summary>
    private static OrderDiscount ToDomain(OrdenesDescuento descuentoEntity)
    {
        return OrderDiscount.Reconstitute(
            id: descuentoEntity.OrdenDescuentoId,
            discountId: descuentoEntity.DescuentoId,
            comboId: descuentoEntity.ComboId,
            discountAmount: Money.FromDecimal(descuentoEntity.MontoDescuento),
            appliedBy: descuentoEntity.AplicadoPor,
            justification: descuentoEntity.Justificacion
        );
    }
}
