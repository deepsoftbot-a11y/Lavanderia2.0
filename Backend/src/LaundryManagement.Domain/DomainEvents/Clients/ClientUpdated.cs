using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Clients;

/// <summary>
/// Evento de dominio que se dispara cuando se actualiza la información de un cliente
/// </summary>
public sealed class ClientUpdated : DomainEvent
{
    /// <summary>
    /// ID del cliente actualizado
    /// </summary>
    public int ClientId { get; }

    /// <summary>
    /// Fecha y hora de la actualización
    /// </summary>
    public DateTime UpdatedAt { get; }

    public ClientUpdated(int clientId, DateTime updatedAt)
    {
        ClientId = clientId;
        UpdatedAt = updatedAt;
    }
}
