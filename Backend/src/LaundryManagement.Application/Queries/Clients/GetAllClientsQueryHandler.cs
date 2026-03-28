using LaundryManagement.Application.DTOs.Clients;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Clients;

/// <summary>
/// Handler para obtener todos los clientes con filtros y ordenamiento
/// </summary>
public sealed class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, List<ClientDto>>
{
    private readonly IClientRepository _clientRepository;

    public GetAllClientsQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
    }

    public async Task<List<ClientDto>> Handle(GetAllClientsQuery query, CancellationToken ct)
    {
        // 1. Obtener todos los clientes desde el repositorio
        var includeInactive = query.IsActive != true; // Si null o false, incluir inactivos
        var clients = await _clientRepository.GetAllAsync(includeInactive, ct);

        // 2. Filtrar por estado activo si está especificado
        if (query.IsActive.HasValue)
            clients = clients.Where(c => c.IsActive == query.IsActive.Value);

        // 3. Filtrar por búsqueda de texto
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.ToLowerInvariant();
            clients = clients.Where(c =>
                c.Name.ToLowerInvariant().Contains(searchTerm) ||
                c.PhoneNumber.Value.Contains(searchTerm) ||
                c.CustomerNumber.ToLowerInvariant().Contains(searchTerm)
            );
        }

        // 4. Mapear a DTOs
        var dtos = clients.Select(c => new ClientDto
        {
            Id = c.Id.Value,
            CustomerNumber = c.CustomerNumber,
            Name = c.Name,
            Phone = c.PhoneNumber.Value,
            Email = c.Email?.Value,
            Address = c.Address,
            Rfc = c.Rfc?.Value,
            CreditLimit = c.CreditLimit.Amount,
            CurrentBalance = c.CurrentBalance.Amount,
            IsActive = c.IsActive,
            CreatedAt = c.RegisteredAt,
            CreatedBy = c.RegisteredBy,
            HasCreditAvailable = c.HasCreditAvailable,
            AvailableCredit = c.AvailableCredit.Amount
        }).ToList();

        // 5. Ordenar según los parámetros
        dtos = (query.SortBy.ToLowerInvariant(), query.SortOrder.ToLowerInvariant()) switch
        {
            ("name", "desc") => dtos.OrderByDescending(c => c.Name).ToList(),
            ("name", _) => dtos.OrderBy(c => c.Name).ToList(),
            ("createdat", "desc") => dtos.OrderByDescending(c => c.CreatedAt).ToList(),
            ("createdat", _) => dtos.OrderBy(c => c.CreatedAt).ToList(),
            _ => dtos.OrderBy(c => c.Name).ToList() // Default: nombre ascendente
        };

        return dtos;
    }
}
