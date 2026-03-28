using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Application.DTOs.ServicePrices;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.ServicePrices;

/// <summary>
/// Handler para obtener un precio de servicio-prenda por ID
/// </summary>
public sealed class GetServicePriceByIdQueryHandler : IRequestHandler<GetServicePriceByIdQuery, ServicePriceDto?>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceGarmentRepository _garmentRepository;

    public GetServicePriceByIdQueryHandler(
        IServiceRepository serviceRepository,
        IServiceGarmentRepository garmentRepository)
    {
        _serviceRepository = serviceRepository;
        _garmentRepository = garmentRepository;
    }

    public async Task<ServicePriceDto?> Handle(GetServicePriceByIdQuery query, CancellationToken cancellationToken)
    {
        // Obtener todos los servicios y buscar el precio específico
        var services = await _serviceRepository.GetAllAsync(cancellationToken);

        ServicePrice? targetPrice = null;
        int? targetServiceId = null;

        foreach (var service in services)
        {
            var price = service.Prices.FirstOrDefault(p => p.Id.Value == query.ServicePriceId);
            if (price != null)
            {
                targetPrice = price;
                targetServiceId = service.Id.Value;
                break;
            }
        }

        if (targetPrice == null)
            return null;

        // Obtener información de la prenda
        var garment = await _garmentRepository.GetByIdAsync(
            targetPrice.ServiceGarmentId, cancellationToken);

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

        return new ServicePriceDto
        {
            Id = targetPrice.Id.Value,
            ServiceId = targetServiceId!.Value,
            GarmentTypeId = targetPrice.ServiceGarmentId.Value,
            GarmentType = garmentDto,
            UnitPrice = targetPrice.Price.Amount,
            IsActive = targetPrice.IsActive,
            CreatedAt = targetPrice.CreatedAt.ToString("o"),
            UpdatedAt = targetPrice.UpdatedAt?.ToString("o")
        };
    }
}
