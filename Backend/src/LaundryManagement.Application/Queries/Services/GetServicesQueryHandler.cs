using LaundryManagement.Application.Commands.Services;
using LaundryManagement.Application.DTOs.Services;
using LaundryManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.Services;

public sealed class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, List<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<GetServicesQueryHandler> _logger;

    public GetServicesQueryHandler(
        IServiceRepository serviceRepository,
        ILogger<GetServicesQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo servicios con filtros");

            var services = await (query.IsActive.HasValue && query.IsActive.Value
                ? _serviceRepository.GetAllActiveAsync(cancellationToken)
                : _serviceRepository.GetAllAsync(cancellationToken));

            var result = services.Select(CreateServiceCommandHandler.MapToDto).ToList();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchLower = query.Search.ToLowerInvariant();
                result = result.Where(s => s.Name.ToLowerInvariant().Contains(searchLower) ||
                                         (s.Description?.ToLowerInvariant().Contains(searchLower) ?? false))
                              .ToList();
            }

            if (query.CategoryId.HasValue)
            {
                result = result.Where(s => s.CategoryId == query.CategoryId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(query.ChargeType))
            {
                result = result.Where(s => s.ChargeType.Equals(query.ChargeType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener servicios");
            throw;
        }
    }
}
