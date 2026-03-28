using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Services;

/// <summary>
/// Evento de dominio que se dispara cuando cambia el estado activo de un servicio
/// </summary>
public sealed class ServiceStatusChanged : DomainEvent
{
    public int ServiceId { get; }
    public bool IsActive { get; }
    public DateTime ChangedAt { get; }

    public ServiceStatusChanged(int serviceId, bool isActive, DateTime changedAt)
    {
        ServiceId = serviceId;
        IsActive = isActive;
        ChangedAt = changedAt;
    }
}
