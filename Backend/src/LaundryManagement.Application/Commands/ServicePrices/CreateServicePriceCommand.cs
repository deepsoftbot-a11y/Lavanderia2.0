using LaundryManagement.Application.DTOs.ServicePrices;
using MediatR;

namespace LaundryManagement.Application.Commands.ServicePrices;

/// <summary>
/// Comando para crear un nuevo precio servicio-prenda (junction table).
/// </summary>
public sealed record CreateServicePriceCommand : IRequest<ServicePriceDto>
{
    public int ServiceId { get; init; }
    public int GarmentTypeId { get; init; }
    public decimal UnitPrice { get; init; }
}
