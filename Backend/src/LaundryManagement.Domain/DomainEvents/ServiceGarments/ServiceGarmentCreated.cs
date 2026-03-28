using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.ServiceGarments;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo tipo de prenda
/// </summary>
public sealed class ServiceGarmentCreated : DomainEvent
{
    public int ServiceGarmentId { get; }
    public string Name { get; }
    public DateTime CreatedAt { get; }

    public ServiceGarmentCreated(int serviceGarmentId, string name, DateTime createdAt)
    {
        ServiceGarmentId = serviceGarmentId;
        Name = name;
        CreatedAt = createdAt;
    }
}
