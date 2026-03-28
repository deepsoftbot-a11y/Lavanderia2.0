using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Application.DTOs.ServicePrices;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.ServicePrices;

/// <summary>
/// Handler para obtener un precio por la combinación servicio-prenda
/// </summary>
public sealed class GetServicePriceByComboQueryHandler : IRequestHandler<GetServicePriceByComboQuery, ServicePriceDto?>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceGarmentRepository _garmentRepository;
    private readonly ILogger<GetServicePriceByComboQueryHandler> _logger;

    public GetServicePriceByComboQueryHandler(
        IServiceRepository serviceRepository,
        IServiceGarmentRepository garmentRepository,
        ILogger<GetServicePriceByComboQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _garmentRepository = garmentRepository;
        _logger = logger;
    }

    public async Task<ServicePriceDto?> Handle(GetServicePriceByComboQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting service price by combo: ServiceId={ServiceId}, ServiceGarmentId={ServiceGarmentId}",
            query.ServiceId,
            query.ServiceGarmentId
        );

        // Obtener el servicio
        var service = await _serviceRepository.GetByIdAsync(
            ServiceId.From(query.ServiceId),
            cancellationToken
        );

        if (service == null)
        {
            _logger.LogWarning("Service not found: ServiceId={ServiceId}", query.ServiceId);
            return null;
        }

        // Solo servicios basados en piezas tienen precios por prenda
        if (!service.IsPieceBased)
        {
            _logger.LogWarning(
                "Service is not piece-based: ServiceId={ServiceId}",
                query.ServiceId
            );
            return null;
        }

        // Buscar el precio para la prenda específica (activo)
        var price = service.Prices.FirstOrDefault(p =>
            p.ServiceGarmentId.Value == query.ServiceGarmentId &&
            p.IsActive
        );

        if (price == null)
        {
            _logger.LogInformation(
                "No active price found for combo: ServiceId={ServiceId}, ServiceGarmentId={ServiceGarmentId}",
                query.ServiceId,
                query.ServiceGarmentId
            );
            return null;
        }

        // Obtener información de la prenda
        var garment = await _garmentRepository.GetByIdAsync(
            ServiceGarmentId.From(query.ServiceGarmentId),
            cancellationToken
        );

        ServiceGarmentDto? garmentDto = null;
        if (garment != null)
        {
            garmentDto = new ServiceGarmentDto
            {
                Id = garment.Id.Value,
                Name = garment.Name,
                Description = garment.Description,
                IsActive = garment.IsActive,
                CreatedAt = garment.CreatedAt.ToString("o"),
                UpdatedAt = garment.UpdatedAt?.ToString("o")
            };
        }

        // Mapear a DTO
        return new ServicePriceDto
        {
            Id = price.Id.Value,
            ServiceId = service.Id.Value,
            GarmentTypeId = price.ServiceGarmentId.Value,
            GarmentType = garmentDto,
            UnitPrice = price.Price.Amount,
            IsActive = price.IsActive,
            CreatedAt = price.CreatedAt.ToString("o"),
            UpdatedAt = price.UpdatedAt?.ToString("o")
        };
    }
}
