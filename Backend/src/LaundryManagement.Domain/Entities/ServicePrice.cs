using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un precio específico de un servicio para un tipo de prenda.
/// Esta es una child entity dentro del agregado ServicePure.
/// Solo aplica para servicios de tipo PIECE (cobro por pieza).
/// </summary>
public sealed class ServicePrice : Entity<ServicePriceId>
{
    /// <summary>
    /// Identificador del servicio al que pertenece este precio
    /// </summary>
    public ServiceId ServiceId { get; private set; }

    /// <summary>
    /// Identificador del tipo de prenda
    /// </summary>
    public ServiceGarmentId ServiceGarmentId { get; private set; }

    /// <summary>
    /// Precio para esta combinación servicio-prenda
    /// </summary>
    public Money Price { get; private set; }

    /// <summary>
    /// Indica si el precio está activo
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Constructor privado para EF Core y reconstitución
    /// </summary>
    private ServicePrice()
    {
        ServiceId = ServiceId.Empty();
        ServiceGarmentId = ServiceGarmentId.Empty();
        Price = Money.Zero();
    }

    /// <summary>
    /// Crea un nuevo precio de servicio-prenda
    /// </summary>
    internal static ServicePrice Create(
        ServiceId serviceId,
        ServiceGarmentId serviceGarmentId,
        Money price)
    {
        // Validaciones
        if (serviceId.IsEmpty)
            throw new ValidationException("El ServiceId es requerido");

        if (serviceGarmentId.IsEmpty)
            throw new ValidationException("El ServiceGarmentId es requerido");

        if (price.IsZero || price.Amount <= 0)
            throw new ValidationException("El precio debe ser mayor que cero");

        var servicePrice = new ServicePrice
        {
            Id = ServicePriceId.Empty(),
            ServiceId = serviceId,
            ServiceGarmentId = serviceGarmentId,
            Price = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        return servicePrice;
    }

    /// <summary>
    /// Reconstituye un ServicePrice desde la base de datos (usado por Repository)
    /// </summary>
    internal static ServicePrice Reconstitute(
        ServicePriceId id,
        ServiceId serviceId,
        ServiceGarmentId serviceGarmentId,
        Money price,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new ServicePrice
        {
            Id = id,
            ServiceId = serviceId,
            ServiceGarmentId = serviceGarmentId,
            Price = price,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    /// <summary>
    /// Actualiza el precio
    /// </summary>
    internal void UpdatePrice(Money newPrice)
    {
        if (newPrice.IsZero || newPrice.Amount <= 0)
            throw new ValidationException("El precio debe ser mayor que cero");

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activa el precio
    /// </summary>
    internal void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desactiva el precio
    /// </summary>
    internal void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Establece el ID (usado por el aggregate root después de persistir)
    /// </summary>
    internal void SetId(ServicePriceId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido establecido");

        Id = id;
    }
}
