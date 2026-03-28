using MediatR;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Comando para alternar el estado activo/inactivo de un cliente
/// </summary>
public sealed record ToggleClientStatusCommand : IRequest<ToggleClientStatusResult>
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public int ClientId { get; init; }

    public ToggleClientStatusCommand(int clientId)
    {
        ClientId = clientId;
    }
}

/// <summary>
/// Resultado del cambio de estado
/// </summary>
public sealed record ToggleClientStatusResult
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public int ClientId { get; init; }

    /// <summary>
    /// Nuevo estado activo (true) o inactivo (false)
    /// </summary>
    public bool IsActive { get; init; }
}
