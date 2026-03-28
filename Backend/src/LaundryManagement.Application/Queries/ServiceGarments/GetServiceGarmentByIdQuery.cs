using LaundryManagement.Application.DTOs.ServiceGarments;
using MediatR;

namespace LaundryManagement.Application.Queries.ServiceGarments;

/// <summary>
/// Query para obtener un tipo de prenda por su ID
/// </summary>
public sealed record GetServiceGarmentByIdQuery : IRequest<ServiceGarmentDto?>
{
    /// <summary>
    /// ID del tipo de prenda
    /// </summary>
    public int ServiceGarmentId { get; init; }

    public GetServiceGarmentByIdQuery(int serviceGarmentId)
    {
        ServiceGarmentId = serviceGarmentId;
    }
}
