using MediatR;

namespace LaundryManagement.Application.Commands.ServicePrices;

/// <summary>
/// Comando para eliminar (desactivar) un precio servicio-prenda por su ID.
/// </summary>
public sealed record DeleteServicePriceCommand : IRequest<Unit>
{
    public int ServicePriceId { get; init; }
}
