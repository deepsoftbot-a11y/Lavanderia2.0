using MediatR;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Comando para desactivar un cliente (soft delete)
/// </summary>
public sealed record DeactivateClientCommand : IRequest<Unit>
{
    /// <summary>
    /// ID del cliente a desactivar
    /// </summary>
    public int ClientId { get; init; }

    public DeactivateClientCommand(int clientId)
    {
        ClientId = clientId;
    }
}
