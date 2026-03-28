using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.DomainEvents.Orders;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Orders;

/// <summary>
/// Agregado Order PURO - Entidad de dominio rica completamente independiente de infraestructura.
/// NO conoce EF Core, NO conoce base de datos, solo lógica de negocio.
/// </summary>
public sealed class OrderPure : AggregateRoot<OrderId>
{
    private readonly List<OrderLineItem> _lineItems;
    private readonly List<OrderDiscount> _discounts;

    #region Propiedades de Dominio

    /// <summary>
    /// Folio único de la orden
    /// </summary>
    public OrderFolio Folio { get; private set; }

    /// <summary>
    /// Identificador del cliente
    /// </summary>
    public ClientId ClientId { get; private set; }

    /// <summary>
    /// Fecha de recepción
    /// </summary>
    public DateTime ReceivedDate { get; private set; }

    /// <summary>
    /// Fecha prometida de entrega
    /// </summary>
    public DateTime PromisedDate { get; private set; }

    /// <summary>
    /// Fecha de entrega real
    /// </summary>
    public DateTime? DeliveryDate { get; private set; }

    /// <summary>
    /// Estado actual de la orden
    /// </summary>
    public int StatusId { get; private set; }

    /// <summary>
    /// Subtotal (suma de todos los items)
    /// </summary>
    public Money Subtotal { get; private set; }

    /// <summary>
    /// Descuento total
    /// </summary>
    public Money TotalDiscount { get; private set; }

    /// <summary>
    /// Total (Subtotal - Descuento)
    /// </summary>
    public Money Total { get; private set; }

    /// <summary>
    /// Observaciones
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Ubicación de almacenamiento
    /// </summary>
    public string? StorageLocation { get; private set; }

    /// <summary>
    /// Usuario que recibió la orden
    /// </summary>
    public int ReceivedBy { get; private set; }

    /// <summary>
    /// Usuario que entregó la orden
    /// </summary>
    public int? DeliveredBy { get; private set; }

    /// <summary>
    /// Items de la orden (solo lectura)
    /// </summary>
    public IReadOnlyCollection<OrderLineItem> LineItems => _lineItems.AsReadOnly();

    /// <summary>
    /// Descuentos aplicados (solo lectura)
    /// </summary>
    public IReadOnlyCollection<OrderDiscount> Discounts => _discounts.AsReadOnly();

    /// <summary>
    /// Verifica si la orden está entregada
    /// </summary>
    public bool IsDelivered => DeliveryDate.HasValue;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado para reconstitución desde BD
    /// </summary>
    private OrderPure()
    {
        _lineItems = new List<OrderLineItem>();
        _discounts = new List<OrderDiscount>();
        Folio = OrderFolio.FromString("ORD-00000000-0000");
        ClientId = ClientId.From(1);
        Subtotal = Money.Zero();
        TotalDiscount = Money.Zero();
        Total = Money.Zero();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea una nueva orden
    /// </summary>
    public static OrderPure Create(
        ClientId clientId,
        DateTime promisedDate,
        int receivedBy,
        int initialStatusId,
        string? notes = null,
        string? storageLocation = null)
    {
        // Validaciones
        if (promisedDate.Date < DateTime.Today)
            throw new BusinessRuleException("La fecha prometida no puede ser en el pasado");

        if (receivedBy <= 0)
            throw new ValidationException("ReceivedBy debe ser un usuario válido");

        if (initialStatusId <= 0)
            throw new ValidationException("El estado inicial debe ser válido");

        var order = new OrderPure
        {
            Id = OrderId.Empty(), // Se asignará al persistir
            Folio = OrderFolio.FromString("ORD-00000000-0000"), // Se asignará por Repository
            ClientId = clientId,
            ReceivedDate = DateTime.Now,
            PromisedDate = promisedDate,
            StatusId = initialStatusId,
            Subtotal = Money.Zero(),
            TotalDiscount = Money.Zero(),
            Total = Money.Zero(),
            ReceivedBy = receivedBy,
            Notes = notes,
            StorageLocation = storageLocation
        };

        // Evento de dominio
        order.RaiseDomainEvent(new OrderCreated(
            0, // Se actualizará al persistir
            clientId.Value,
            receivedBy,
            DateTime.Now
        ));

        return order;
    }

    /// <summary>
    /// Reconstituye una orden desde la base de datos (usado por Repository)
    /// </summary>
    public static OrderPure Reconstitute(
        OrderId id,
        OrderFolio folio,
        ClientId clientId,
        DateTime receivedDate,
        DateTime promisedDate,
        DateTime? deliveryDate,
        int statusId,
        Money subtotal,
        Money totalDiscount,
        Money total,
        string? notes,
        string? storageLocation,
        int receivedBy,
        int? deliveredBy,
        List<OrderLineItem> lineItems,
        List<OrderDiscount> discounts)
    {
        var order = new OrderPure
        {
            Id = id,
            Folio = folio,
            ClientId = clientId,
            ReceivedDate = receivedDate,
            PromisedDate = promisedDate,
            DeliveryDate = deliveryDate,
            StatusId = statusId,
            Subtotal = subtotal,
            TotalDiscount = totalDiscount,
            Total = total,
            Notes = notes,
            StorageLocation = storageLocation,
            ReceivedBy = receivedBy,
            DeliveredBy = deliveredBy
        };

        order._lineItems.AddRange(lineItems);
        order._discounts.AddRange(discounts);

        return order;
    }

    #endregion

    #region Métodos de Dominio

    /// <summary>
    /// Agrega un item a la orden
    /// </summary>
    public void AddLineItem(
        int serviceId,
        int? serviceGarmentId,
        decimal? weightKilos,
        int? quantity,
        Money unitPrice,
        Money? lineDiscount = null,
        string? notes = null)
    {
        // Validar que se puede modificar
        EnsureCanBeModified();

        // Calcular número de línea
        int lineNumber = _lineItems.Any()
            ? _lineItems.Max(li => li.LineNumber) + 1
            : 1;

        // Crear el line item
        var lineItem = OrderLineItem.Create(
            lineNumber,
            serviceId,
            serviceGarmentId,
            weightKilos,
            quantity,
            unitPrice,
            notes
        );

        // Aplicar descuento si se proporciona
        if (lineDiscount != null && !lineDiscount.IsZero)
        {
            lineItem.ApplyDiscount(lineDiscount);
        }

        // Agregar a la colección
        _lineItems.Add(lineItem);

        // Recalcular totales
        RecalculateTotals();

        // Evento de dominio
        RaiseDomainEvent(new OrderLineItemAdded(
            Id.Value,
            0, // Se asignará al persistir
            serviceId,
            weightKilos,
            quantity,
            unitPrice.Amount
        ));
    }

    /// <summary>
    /// Aplica un descuento a la orden
    /// </summary>
    public void ApplyDiscount(
        int? discountId,
        int? comboId,
        Money discountAmount,
        int appliedBy,
        string? justification = null)
    {
        // Validar que se puede modificar
        EnsureCanBeModified();

        // Validar que el descuento no exceda el subtotal
        if (discountAmount.IsGreaterThan(Subtotal))
            throw new BusinessRuleException("El descuento no puede exceder el subtotal de la orden");

        // Crear el descuento
        var discount = OrderDiscount.Create(
            discountId,
            comboId,
            discountAmount,
            appliedBy,
            justification
        );

        // Agregar a la colección
        _discounts.Add(discount);

        // Recalcular totales
        RecalculateTotals();

        // Evento de dominio
        RaiseDomainEvent(new OrderDiscountApplied(
            Id.Value,
            discountId,
            comboId,
            discountAmount.Amount,
            appliedBy
        ));
    }

    /// <summary>
    /// Cambia el estado de la orden
    /// </summary>
    public void ChangeStatus(int newStatusId, int changedBy, string? comments = null)
    {
        if (newStatusId <= 0)
            throw new ValidationException("El EstadoOrdenId debe ser válido");

        if (changedBy <= 0)
            throw new ValidationException("ChangedBy debe ser un usuario válido");

        int previousStatus = StatusId;

        if (previousStatus == newStatusId)
            return;

        // Validar transición
        ValidateStatusTransition(previousStatus, newStatusId);

        // Cambiar estado
        StatusId = newStatusId;

        // Evento de dominio
        RaiseDomainEvent(new OrderStatusChanged(
            Id.Value,
            previousStatus,
            newStatusId,
            changedBy,
            DateTime.Now
        ));
    }

    /// <summary>
    /// Marca la orden como entregada
    /// </summary>
    public void MarkAsDelivered(int deliveredBy, DateTime? deliveryDate = null)
    {
        if (deliveredBy <= 0)
            throw new ValidationException("DeliveredBy debe ser un usuario válido");

        if (DeliveryDate.HasValue)
            throw new BusinessRuleException("La orden ya fue entregada");

        if (!_lineItems.Any())
            throw new BusinessRuleException("No se puede entregar una orden sin items");

        DeliveryDate = deliveryDate ?? DateTime.Now;
        DeliveredBy = deliveredBy;

        RaiseDomainEvent(new OrderDelivered(
            Id.Value,
            DeliveryDate.Value,
            deliveredBy
        ));
    }

    /// <summary>
    /// Recalcula los totales de la orden
    /// </summary>
    public void RecalculateTotals()
    {
        var previousTotal = Total;

        // Calcular subtotal desde line items (SIN descuentos de línea)
        Subtotal = _lineItems.Any()
            ? _lineItems.Select(li => li.Subtotal).Aggregate((a, b) => a + b)
            : Money.Zero();

        // Calcular descuento total: descuentos de línea + descuentos de orden
        var lineDiscountsTotal = _lineItems.Any()
            ? _lineItems.Select(li => li.LineDiscount).Aggregate((a, b) => a + b)
            : Money.Zero();

        var orderDiscountsTotal = _discounts.Any()
            ? _discounts.Select(d => d.DiscountAmount).Aggregate((a, b) => a + b)
            : Money.Zero();

        TotalDiscount = lineDiscountsTotal + orderDiscountsTotal;

        // Calcular total
        Total = Subtotal - TotalDiscount;

        // Evento si cambió
        if (!Total.Equals(previousTotal))
        {
            RaiseDomainEvent(new OrderTotalRecalculated(
                Id.Value,
                previousTotal.Amount,
                Total.Amount
            ));
        }
    }

    /// <summary>
    /// Actualiza los datos editables de la orden
    /// </summary>
    public void UpdateDetails(
        DateTime? promisedDate,
        string? notes,
        string? storageLocation)
    {
        EnsureCanBeModified();

        if (promisedDate.HasValue)
        {
            if (promisedDate.Value.Date < DateTime.Today)
                throw new BusinessRuleException("La fecha prometida no puede ser en el pasado");
            PromisedDate = promisedDate.Value;
        }

        if (notes != null) Notes = notes;
        if (storageLocation != null) StorageLocation = storageLocation;
    }

    /// <summary>
    /// Reemplaza todos los line items de la orden
    /// </summary>
    public void ReplaceLineItems(
        IEnumerable<(int serviceId, int? serviceGarmentId, decimal? weightKilos,
                     int? quantity, Money unitPrice, Money lineDiscount, string? notes)> newItems)
    {
        EnsureCanBeModified();

        var itemsList = newItems.ToList();
        if (!itemsList.Any())
            throw new BusinessRuleException("La orden debe tener al menos un item");

        _lineItems.Clear();

        int lineNumber = 1;
        foreach (var item in itemsList)
        {
            var lineItem = OrderLineItem.Create(
                lineNumber,
                item.serviceId,
                item.serviceGarmentId,
                item.weightKilos,
                item.quantity,
                item.unitPrice,
                item.notes
            );

            if (!item.lineDiscount.IsZero)
                lineItem.ApplyDiscount(item.lineDiscount);

            _lineItems.Add(lineItem);
            lineNumber++;
        }

        RecalculateTotals();
    }

    /// <summary>
    /// Cancela la orden (borrado lógico — cambia estado a Cancelada)
    /// </summary>
    public void Cancel(int cancelledBy)
    {
        if (IsDelivered)
            throw new BusinessRuleException("No se puede cancelar una orden que ya fue entregada");

        const int CancelledStatusId = 5;
        ChangeStatus(CancelledStatusId, cancelledBy);
    }

    /// <summary>
    /// Asigna el ID después de persistir (usado por Repository)
    /// </summary>
    internal void SetId(OrderId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido asignado");

        Id = id;
    }

    /// <summary>
    /// Asigna el folio (usado por Repository)
    /// </summary>
    internal void SetFolio(OrderFolio folio)
    {
        Folio = folio;
    }

    #endregion

    #region Validaciones Privadas

    private void EnsureCanBeModified()
    {
        if (DeliveryDate.HasValue)
            throw new BusinessRuleException("No se puede modificar una orden que ya fue entregada");
    }

    private void ValidateStatusTransition(int fromStatusId, int toStatusId)
    {
        // Implementar reglas de transición de estados según negocio
        // Por ahora permitimos todas las transiciones
    }

    #endregion
}
