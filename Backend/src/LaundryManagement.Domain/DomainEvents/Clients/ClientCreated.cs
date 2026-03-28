using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Clients;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo cliente
/// </summary>
public sealed class ClientCreated : DomainEvent
{
    /// <summary>
    /// ID del cliente creado
    /// </summary>
    public int ClientId { get; }

    /// <summary>
    /// Número telefónico del cliente
    /// </summary>
    public string PhoneNumber { get; }

    /// <summary>
    /// ID del usuario que registró el cliente
    /// </summary>
    public int RegisteredBy { get; }

    /// <summary>
    /// Fecha y hora de registro
    /// </summary>
    public DateTime RegisteredAt { get; }

    public ClientCreated(
        int clientId,
        string phoneNumber,
        int registeredBy,
        DateTime registeredAt)
    {
        ClientId = clientId;
        PhoneNumber = phoneNumber;
        RegisteredBy = registeredBy;
        RegisteredAt = registeredAt;
    }
}
