using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Handler para actualizar un cliente existente
/// </summary>
public sealed class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, UpdateClientResult>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<UpdateClientCommandHandler> _logger;

    public UpdateClientCommandHandler(
        IClientRepository clientRepository,
        ILogger<UpdateClientCommandHandler> logger)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateClientResult> Handle(UpdateClientCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation(
            "Updating client: ClientId={ClientId}",
            cmd.ClientId
        );

        // 1. Cargar agregado desde el repositorio
        var client = await _clientRepository.GetByIdAsync(ClientId.From(cmd.ClientId), ct)
            ?? throw new NotFoundException($"Cliente con ID {cmd.ClientId} no encontrado");

        // 2. Validar unicidad de teléfono si está cambiando
        if (!string.IsNullOrWhiteSpace(cmd.PhoneNumber))
        {
            var newPhone = PhoneNumber.From(cmd.PhoneNumber);

            // Solo verificar unicidad si el teléfono es diferente al actual
            if (!newPhone.Equals(client.PhoneNumber))
            {
                if (await _clientRepository.ExistsByPhoneNumberAsync(newPhone, client.Id, ct))
                {
                    throw new BusinessRuleException(
                        $"Ya existe otro cliente con el teléfono {cmd.PhoneNumber}"
                    );
                }
            }
        }

        // 3. Crear value objects para actualización
        var phoneNumber = string.IsNullOrWhiteSpace(cmd.PhoneNumber)
            ? null
            : PhoneNumber.From(cmd.PhoneNumber);

        var email = cmd.Email == null
            ? null
            : (string.IsNullOrWhiteSpace(cmd.Email) ? null : Email.From(cmd.Email));

        var rfc = cmd.Rfc == null
            ? null
            : (string.IsNullOrWhiteSpace(cmd.Rfc) ? null : RFC.From(cmd.Rfc));

        var creditLimit = cmd.CreditLimit.HasValue
            ? Money.FromDecimal(cmd.CreditLimit.Value)
            : null;

        // 4. Actualizar vía método de dominio
        client.UpdateInformation(
            name: cmd.Name,
            phoneNumber: phoneNumber,
            email: email,
            address: cmd.Address,
            rfc: rfc,
            creditLimit: creditLimit
        );

        // 5. Persistir cambios
        await _clientRepository.UpdateAsync(client, ct);
        await _clientRepository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Client updated successfully: ClientId={ClientId}",
            client.Id.Value
        );

        return new UpdateClientResult
        {
            ClientId = client.Id.Value,
            CustomerNumber = client.CustomerNumber,
            Name = client.Name
        };
    }
}
