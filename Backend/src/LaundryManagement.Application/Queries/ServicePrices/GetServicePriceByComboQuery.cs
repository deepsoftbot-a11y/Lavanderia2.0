using LaundryManagement.Application.DTOs.ServicePrices;
using MediatR;

namespace LaundryManagement.Application.Queries.ServicePrices;

/// <summary>
/// Query para obtener un precio específico por la combinación de servicio y tipo de prenda
/// </summary>
public sealed record GetServicePriceByComboQuery : IRequest<ServicePriceDto?>
{
    /// <summary>
    /// ID del servicio
    /// </summary>
    public int ServiceId { get; init; }

    /// <summary>
    /// ID del tipo de prenda
    /// </summary>
    public int ServiceGarmentId { get; init; }

    public GetServicePriceByComboQuery(int serviceId, int serviceGarmentId)
    {
        ServiceId = serviceId;
        ServiceGarmentId = serviceGarmentId;
    }
}
