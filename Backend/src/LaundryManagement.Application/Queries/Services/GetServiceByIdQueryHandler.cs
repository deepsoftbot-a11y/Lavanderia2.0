using LaundryManagement.Application.Commands.Services;
using LaundryManagement.Application.DTOs.Services;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.Services;

public sealed class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto?>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<GetServiceByIdQueryHandler> _logger;

    public GetServiceByIdQueryHandler(
        IServiceRepository serviceRepository,
        ILogger<GetServiceByIdQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<ServiceDto?> Handle(GetServiceByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo servicio {ServiceId}", query.ServiceId);

            var service = await _serviceRepository.GetByIdAsync(ServiceId.From(query.ServiceId), cancellationToken);
            if (service == null)
                return null;

            return CreateServiceCommandHandler.MapToDto(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener servicio {ServiceId}", query.ServiceId);
            throw;
        }
    }
}
