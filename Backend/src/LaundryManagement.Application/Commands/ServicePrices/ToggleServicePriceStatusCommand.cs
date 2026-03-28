using MediatR;

namespace LaundryManagement.Application.Commands.ServicePrices;

/// <summary>
/// Comando para alternar el estado activo/inactivo de un precio servicio-prenda.
/// </summary>
public sealed record ToggleServicePriceStatusCommand : IRequest<Unit>
{
    public int ServicePriceId { get; init; }
}
