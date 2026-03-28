using LaundryManagement.Application.DTOs.Clients;
using MediatR;

namespace LaundryManagement.Application.Queries.Clients;

/// <summary>
/// Query para obtener un cliente por su ID
/// </summary>
public sealed record GetClientByIdQuery : IRequest<ClientDto?>
{
    /// <summary>
    /// ID del cliente a buscar
    /// </summary>
    public int ClientId { get; init; }

    public GetClientByIdQuery(int clientId)
    {
        ClientId = clientId;
    }
}
