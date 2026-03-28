using LaundryManagement.Application.DTOs.Clients;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Clients;

/// <summary>
/// Handler para obtener un cliente por su ID
/// </summary>
public sealed class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    private readonly IClientRepository _clientRepository;

    public GetClientByIdQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
    }

    public async Task<ClientDto?> Handle(GetClientByIdQuery query, CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(ClientId.From(query.ClientId), ct);

        if (client == null)
            return null;

        // Mapear agregado de dominio a DTO
        return new ClientDto
        {
            Id = client.Id.Value,
            CustomerNumber = client.CustomerNumber,
            Name = client.Name,
            Phone = client.PhoneNumber.Value,
            Email = client.Email?.Value,
            Address = client.Address,
            Rfc = client.Rfc?.Value,
            CreditLimit = client.CreditLimit.Amount,
            CurrentBalance = client.CurrentBalance.Amount,
            IsActive = client.IsActive,
            CreatedAt = client.RegisteredAt,
            CreatedBy = client.RegisteredBy,
            HasCreditAvailable = client.HasCreditAvailable,
            AvailableCredit = client.AvailableCredit.Amount
        };
    }
}
