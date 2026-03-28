using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Handler para desactivar un cliente (soft delete)
/// </summary>
public sealed class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand, Unit>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<DeactivateClientCommandHandler> _logger;

    public DeactivateClientCommandHandler(
        IClientRepository clientRepository,
        ILogger<DeactivateClientCommandHandler> logger)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeactivateClientCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation(
            "Deactivating client: ClientId={ClientId}",
            cmd.ClientId
        );

        // 1. Cargar agregado
        var client = await _clientRepository.GetByIdAsync(ClientId.From(cmd.ClientId), ct)
            ?? throw new NotFoundException($"Cliente con ID {cmd.ClientId} no encontrado");

        // 2. Desactivar via método de dominio (valida reglas de negocio)
        // Lanza BusinessRuleException si:
        // - El cliente ya está inactivo
        // - El cliente tiene saldo pendiente
        client.Deactivate();

        // 3. Persistir cambios
        await _clientRepository.UpdateAsync(client, ct);
        await _clientRepository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Client deactivated successfully: ClientId={ClientId}",
            client.Id.Value
        );

        return Unit.Value;
    }
}
