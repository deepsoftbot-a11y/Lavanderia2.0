using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.ServiceGarments;

/// <summary>
/// Handler para obtener lista de tipos de prenda con filtros
/// </summary>
public sealed class GetServiceGarmentsQueryHandler : IRequestHandler<GetServiceGarmentsQuery, List<ServiceGarmentDto>>
{
    private readonly IServiceGarmentRepository _repository;
    private readonly ILogger<GetServiceGarmentsQueryHandler> _logger;

    public GetServiceGarmentsQueryHandler(
        IServiceGarmentRepository repository,
        ILogger<GetServiceGarmentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ServiceGarmentDto>> Handle(GetServiceGarmentsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting service garments: Search={Search}, IsActive={IsActive}",
            query.Search,
            query.IsActive
        );

        // Obtener todos los tipos de prenda según el filtro IsActive
        var garments = query.IsActive.HasValue && query.IsActive.Value
            ? await _repository.GetAllActiveAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        // Aplicar filtro de búsqueda si se proporciona
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchLower = query.Search.ToLowerInvariant();
            garments = garments.Where(g =>
                g.Name.ToLowerInvariant().Contains(searchLower) ||
                (g.Description != null && g.Description.ToLowerInvariant().Contains(searchLower))
            );
        }

        // Filtrar por estado si se especifica false (inactivos)
        if (query.IsActive.HasValue && !query.IsActive.Value)
        {
            garments = garments.Where(g => !g.IsActive);
        }

        // Mapear a DTOs
        var result = garments.Select(g => new ServiceGarmentDto
        {
            Id = g.Id.Value,
            Name = g.Name,
            Description = g.Description,
            IsActive = g.IsActive,
            CreatedAt = g.CreatedAt.ToString("o"),
            UpdatedAt = g.UpdatedAt?.ToString("o")
        }).ToList();

        _logger.LogInformation(
            "Found {Count} service garments",
            result.Count
        );

        return result;
    }
}
