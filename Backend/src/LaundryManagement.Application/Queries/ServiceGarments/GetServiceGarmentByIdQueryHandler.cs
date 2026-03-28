using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.ServiceGarments;

/// <summary>
/// Handler para obtener un tipo de prenda por ID usando el patrón DDD
/// </summary>
public sealed class GetServiceGarmentByIdQueryHandler : IRequestHandler<GetServiceGarmentByIdQuery, ServiceGarmentDto?>
{
    private readonly IServiceGarmentRepository _repository;

    public GetServiceGarmentByIdQueryHandler(IServiceGarmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceGarmentDto?> Handle(GetServiceGarmentByIdQuery query, CancellationToken cancellationToken)
    {
        // Obtener el agregado desde el repositorio de dominio
        var garment = await _repository.GetByIdAsync(
            ServiceGarmentId.From(query.ServiceGarmentId),
            cancellationToken
        );

        if (garment == null)
            return null;

        // Mapear el agregado a DTO para la respuesta
        return new ServiceGarmentDto
        {
            Id = garment.Id.Value,
            Name = garment.Name,
            Description = garment.Description,
            IsActive = garment.IsActive,
            CreatedAt = garment.CreatedAt.ToString("o"),
            UpdatedAt = garment.UpdatedAt?.ToString("o")
        };
    }
}
