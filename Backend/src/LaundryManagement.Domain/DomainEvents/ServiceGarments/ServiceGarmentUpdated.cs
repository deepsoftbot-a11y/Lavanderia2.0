using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.ServiceGarments;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza un tipo de prenda
/// </summary>
public sealed class ServiceGarmentUpdated : DomainEvent
{
    public int ServiceGarmentId { get; }
    public string Name { get; }
    public DateTime UpdatedAt { get; }

    public ServiceGarmentUpdated(int serviceGarmentId, string name, DateTime updatedAt)
    {
        ServiceGarmentId = serviceGarmentId;
        Name = name;
        UpdatedAt = updatedAt;
    }
}
