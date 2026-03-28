using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un item/línea dentro de una orden.
/// Esta es una entidad PURA de dominio, sin dependencias de infraestructura.
/// </summary>
public sealed class OrderLineItem : Entity<int>
{
    /// <summary>
    /// Número de línea secuencial dentro de la orden
    /// </summary>
    public int LineNumber { get; private set; }

    /// <summary>
    /// Identificador del servicio
    /// </summary>
    public int ServiceId { get; private set; }

    /// <summary>
    /// Identificador del servicio-prenda (opcional)
    /// </summary>
    public int? ServiceGarmentId { get; private set; }

    /// <summary>
    /// Peso en kilos (para servicios por peso)
    /// </summary>
    public decimal? WeightKilos { get; private set; }

    /// <summary>
    /// Cantidad de piezas (para servicios por pieza)
    /// </summary>
    public int? Quantity { get; private set; }

    /// <summary>
    /// Precio unitario
    /// </summary>
    public Money UnitPrice { get; private set; }

    /// <summary>
    /// Subtotal de la línea
    /// </summary>
    public Money Subtotal { get; private set; }

    /// <summary>
    /// Descuento aplicado a la línea
    /// </summary>
    public Money LineDiscount { get; private set; }

    /// <summary>
    /// Total de la línea (Subtotal - Descuento)
    /// </summary>
    public Money LineTotal { get; private set; }

    /// <summary>
    /// Observaciones de la línea
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Constructor privado para EF Core y reconstitución
    /// </summary>
    private OrderLineItem()
    {
        UnitPrice = Money.Zero();
        Subtotal = Money.Zero();
        LineDiscount = Money.Zero();
        LineTotal = Money.Zero();
    }

    /// <summary>
    /// Crea un nuevo item de orden
    /// </summary>
    internal static OrderLineItem Create(
        int lineNumber,
        int serviceId,
        int? serviceGarmentId,
        decimal? weightKilos,
        int? quantity,
        Money unitPrice,
        string? notes = null)
    {
        // Validaciones
        if (lineNumber <= 0)
            throw new ValidationException("El número de línea debe ser mayor a cero");

        if (serviceId <= 0)
            throw new ValidationException("El ServiceId debe ser válido");

        // Validar que al menos uno (peso O cantidad) sea mayor a cero
        bool hasValidWeight = weightKilos.HasValue && weightKilos.Value > 0;
        bool hasValidQuantity = quantity.HasValue && quantity.Value > 0;

        if (!hasValidWeight && !hasValidQuantity)
            throw new BusinessRuleException(
                "Debe especificar un peso mayor a cero O una cantidad mayor a cero"
            );

        // Calcular subtotal: priorizar peso si es válido, sino usar cantidad
        decimal baseQuantity = hasValidWeight ? weightKilos!.Value : quantity!.Value;
        var subtotal = unitPrice * baseQuantity;

        var lineItem = new OrderLineItem
        {
            LineNumber = lineNumber,
            ServiceId = serviceId,
            ServiceGarmentId = serviceGarmentId,
            WeightKilos = weightKilos,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal,
            LineDiscount = Money.Zero(),
            LineTotal = subtotal,
            Notes = notes
        };

        return lineItem;
    }

    /// <summary>
    /// Reconstituye un OrderLineItem desde la base de datos (usado por Repository)
    /// </summary>
    internal static OrderLineItem Reconstitute(
        int id,
        int lineNumber,
        int serviceId,
        int? serviceGarmentId,
        decimal? weightKilos,
        int? quantity,
        Money unitPrice,
        Money subtotal,
        Money lineDiscount,
        Money lineTotal,
        string? notes)
    {
        return new OrderLineItem
        {
            Id = id,
            LineNumber = lineNumber,
            ServiceId = serviceId,
            ServiceGarmentId = serviceGarmentId,
            WeightKilos = weightKilos,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal,
            LineDiscount = lineDiscount,
            LineTotal = lineTotal,
            Notes = notes
        };
    }

    /// <summary>
    /// Aplica un descuento a la línea
    /// </summary>
    internal void ApplyDiscount(Money discountAmount)
    {
        if (discountAmount.IsGreaterThan(Subtotal))
            throw new BusinessRuleException("El descuento no puede exceder el subtotal de la línea");

        LineDiscount = discountAmount;
        LineTotal = Subtotal - LineDiscount;
    }

    /// <summary>
    /// Recalcula el total de la línea
    /// </summary>
    internal void RecalculateTotal()
    {
        // Priorizar peso si es válido, sino usar cantidad
        bool hasValidWeight = WeightKilos.HasValue && WeightKilos.Value > 0;
        decimal baseQuantity = hasValidWeight ? WeightKilos!.Value : (Quantity ?? 0);

        Subtotal = UnitPrice * baseQuantity;
        LineTotal = Subtotal - LineDiscount;
    }
}
