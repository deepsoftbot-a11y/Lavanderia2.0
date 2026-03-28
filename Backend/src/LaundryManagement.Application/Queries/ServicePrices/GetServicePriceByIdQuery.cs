using LaundryManagement.Application.DTOs.ServicePrices;
using MediatR;

namespace LaundryManagement.Application.Queries.ServicePrices;

/// <summary>
/// Query para obtener un precio de servicio-prenda por su ID
/// </summary>
public sealed record GetServicePriceByIdQuery : IRequest<ServicePriceDto?>
{
    /// <summary>
    /// ID del precio de servicio-prenda
    /// </summary>
    public int ServicePriceId { get; init; }

    public GetServicePriceByIdQuery(int servicePriceId)
    {
        ServicePriceId = servicePriceId;
    }
}
