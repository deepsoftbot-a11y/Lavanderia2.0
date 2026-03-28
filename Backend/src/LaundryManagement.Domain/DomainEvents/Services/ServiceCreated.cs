using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.DomainEvents.Services;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo servicio
/// </summary>
public sealed class ServiceCreated : DomainEvent
{
    public int ServiceId { get; }
    public string Code { get; }
    public string Name { get; }
    public int CategoryId { get; }
    public UnitType.Type UnitType { get; }
    public DateTime CreatedAt { get; }

    public ServiceCreated(
        int serviceId,
        string code,
        string name,
        int categoryId,
        UnitType.Type unitType,
        DateTime createdAt)
    {
        ServiceId = serviceId;
        Code = code;
        Name = name;
        CategoryId = categoryId;
        UnitType = unitType;
        CreatedAt = createdAt;
    }
}
