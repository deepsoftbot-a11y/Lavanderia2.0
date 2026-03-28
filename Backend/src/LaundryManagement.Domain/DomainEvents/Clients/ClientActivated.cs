using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Clients;

/// <summary>
/// Evento de dominio que se dispara cuando se activa un cliente
/// </summary>
public sealed class ClientActivated : DomainEvent
{
    /// <summary>
    /// ID del cliente activado
    /// </summary>
    public int ClientId { get; }

    /// <summary>
    /// Fecha y hora de activación
    /// </summary>
    public DateTime ActivatedAt { get; }

    public ClientActivated(int clientId, DateTime activatedAt)
    {
        ClientId = clientId;
        ActivatedAt = activatedAt;
    }
}
