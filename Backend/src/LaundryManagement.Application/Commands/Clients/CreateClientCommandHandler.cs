using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Handler que implementa el patrón DDD para crear clientes.
/// Usa el agregado ClientPure y el repositorio de dominio.
/// </summary>
public sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, CreateClientResult>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<CreateClientCommandHandler> _logger;

    public CreateClientCommandHandler(
        IClientRepository clientRepository,
        ILogger<CreateClientCommandHandler> logger)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateClientResult> Handle(CreateClientCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation(
            "Creating client: Name={Name}, Phone={Phone}",
            cmd.Name,
            cmd.Phone
        );

        // 1. Crear value objects
        var phoneNumber = PhoneNumber.From(cmd.Phone);

        // 2. Validar unicidad de teléfono
        if (await _clientRepository.ExistsByPhoneNumberAsync(phoneNumber, null, ct))
        {
            throw new BusinessRuleException(
                $"Ya existe un cliente con el teléfono {cmd.Phone}"
            );
        }

        // 3. Crear value objects opcionales
        var email = string.IsNullOrWhiteSpace(cmd.Email)
            ? null
            : Email.From(cmd.Email);

        var rfc = string.IsNullOrWhiteSpace(cmd.Rfc)
            ? null
            : RFC.From(cmd.Rfc);

        var creditLimit = cmd.CreditLimit.HasValue
            ? Money.FromDecimal(cmd.CreditLimit.Value)
            : Money.Zero();

        // TODO: Obtener de JWT token cuando se implemente autenticación
        int registeredBy = 1; // Hardcoded temporalmente

        // 4. Crear agregado via factory method
        var client = ClientPure.Create(
            name: cmd.Name,
            phoneNumber: phoneNumber,
            registeredBy: registeredBy,
            email: email,
            address: cmd.Address,
            rfc: rfc,
            creditLimit: creditLimit
        );

        // 5. Persistir (el trigger de BD generará NumeroCliente automáticamente)
        var savedClient = await _clientRepository.AddAsync(client, ct);
        await _clientRepository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Client created successfully: ID={ClientId}, CustomerNumber={CustomerNumber}",
            savedClient.Id.Value,
            savedClient.CustomerNumber
        );

        return new CreateClientResult
        {
            Id = savedClient.Id.Value,
            CustomerNumber = savedClient.CustomerNumber,
            Name = savedClient.Name,
            Phone = savedClient.PhoneNumber.Value,
            Email = savedClient.Email?.Value,
            RegisteredAt = savedClient.RegisteredAt
        };
    }
}
