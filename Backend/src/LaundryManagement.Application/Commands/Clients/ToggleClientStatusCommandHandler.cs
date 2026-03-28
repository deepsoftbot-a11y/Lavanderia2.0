using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Handler para alternar el estado activo/inactivo de un cliente
/// </summary>
public sealed class ToggleClientStatusCommandHandler : IRequestHandler<ToggleClientStatusCommand, ToggleClientStatusResult>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ToggleClientStatusCommandHandler> _logger;

    public ToggleClientStatusCommandHandler(
        IClientRepository clientRepository,
        ILogger<ToggleClientStatusCommandHandler> logger)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ToggleClientStatusResult> Handle(ToggleClientStatusCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation(
            "Toggling client status: ClientId={ClientId}",
            cmd.ClientId
        );

        // 1. Cargar agregado
        var client = await _clientRepository.GetByIdAsync(ClientId.From(cmd.ClientId), ct)
            ?? throw new NotFoundException($"Cliente con ID {cmd.ClientId} no encontrado");

        // 2. Alternar estado via métodos de dominio
        if (client.IsActive)
        {
            // Desactivar (valida que no tenga saldo pendiente)
            client.Deactivate();
        }
        else
        {
            // Activar
            client.Activate();
        }

        // 3. Persistir cambios
        await _clientRepository.UpdateAsync(client, ct);
        await _clientRepository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Client status toggled successfully: ClientId={ClientId}, IsActive={IsActive}",
            client.Id.Value,
            client.IsActive
        );

        return new ToggleClientStatusResult
        {
            ClientId = client.Id.Value,
            IsActive = client.IsActive
        };
    }
}
