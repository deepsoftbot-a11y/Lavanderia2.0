using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Clients;

/// <summary>
/// Evento de dominio que se dispara cuando cambia el saldo de un cliente
/// (generalmente después de operaciones de pago)
/// </summary>
public sealed class ClientBalanceChanged : DomainEvent
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public int ClientId { get; }

    /// <summary>
    /// Saldo previo
    /// </summary>
    public decimal PreviousBalance { get; }

    /// <summary>
    /// Nuevo saldo
    /// </summary>
    public decimal NewBalance { get; }

    /// <summary>
    /// Fecha y hora del cambio
    /// </summary>
    public DateTime ChangedAt { get; }

    public ClientBalanceChanged(
        int clientId,
        decimal previousBalance,
        decimal newBalance,
        DateTime changedAt)
    {
        ClientId = clientId;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        ChangedAt = changedAt;
    }
}
