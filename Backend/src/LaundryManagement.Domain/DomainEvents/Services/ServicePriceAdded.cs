using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Services;

/// <summary>
/// Evento de dominio que se dispara cuando se agrega un precio a un servicio para un tipo de prenda
/// </summary>
public sealed class ServicePriceAdded : DomainEvent
{
    public int ServiceId { get; }
    public int ServicePriceId { get; }
    public int ServiceGarmentId { get; }
    public decimal Price { get; }
    public DateTime AddedAt { get; }

    public ServicePriceAdded(
        int serviceId,
        int servicePriceId,
        int serviceGarmentId,
        decimal price,
        DateTime addedAt)
    {
        ServiceId = serviceId;
        ServicePriceId = servicePriceId;
        ServiceGarmentId = serviceGarmentId;
        Price = price;
        AddedAt = addedAt;
    }
}
