using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Services;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza el precio de un servicio para un tipo de prenda
/// </summary>
public sealed class ServicePriceUpdated : DomainEvent
{
    public int ServiceId { get; }
    public int ServicePriceId { get; }
    public int ServiceGarmentId { get; }
    public decimal NewPrice { get; }
    public DateTime UpdatedAt { get; }

    public ServicePriceUpdated(
        int serviceId,
        int servicePriceId,
        int serviceGarmentId,
        decimal newPrice,
        DateTime updatedAt)
    {
        ServiceId = serviceId;
        ServicePriceId = servicePriceId;
        ServiceGarmentId = serviceGarmentId;
        NewPrice = newPrice;
        UpdatedAt = updatedAt;
    }
}
