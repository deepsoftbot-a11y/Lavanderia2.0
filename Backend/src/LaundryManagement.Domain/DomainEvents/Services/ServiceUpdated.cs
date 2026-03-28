using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Services;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza un servicio
/// </summary>
public sealed class ServiceUpdated : DomainEvent
{
    public int ServiceId { get; }
    public string Name { get; }
    public DateTime UpdatedAt { get; }

    public ServiceUpdated(int serviceId, string name, DateTime updatedAt)
    {
        ServiceId = serviceId;
        Name = name;
        UpdatedAt = updatedAt;
    }
}
