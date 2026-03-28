using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Clients;

/// <summary>
/// Evento de dominio que se dispara cuando se desactiva un cliente (soft delete)
/// </summary>
public sealed class ClientDeactivated : DomainEvent
{
    /// <summary>
    /// ID del cliente desactivado
    /// </summary>
    public int ClientId { get; }

    /// <summary>
    /// Fecha y hora de desactivación
    /// </summary>
    public DateTime DeactivatedAt { get; }

    public ClientDeactivated(int clientId, DateTime deactivatedAt)
    {
        ClientId = clientId;
        DeactivatedAt = deactivatedAt;
    }
}
