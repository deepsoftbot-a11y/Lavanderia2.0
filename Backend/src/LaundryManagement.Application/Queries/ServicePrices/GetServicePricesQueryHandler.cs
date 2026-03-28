using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Application.DTOs.ServicePrices;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.ServicePrices;

/// <summary>
/// Handler para obtener precios de servicio-prenda con filtros
/// </summary>
public sealed class GetServicePricesQueryHandler : IRequestHandler<GetServicePricesQuery, List<ServicePriceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceGarmentRepository _garmentRepository;
    private readonly ILogger<GetServicePricesQueryHandler> _logger;

    public GetServicePricesQueryHandler(
        IServiceRepository serviceRepository,
        IServiceGarmentRepository garmentRepository,
        ILogger<GetServicePricesQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _garmentRepository = garmentRepository;
        _logger = logger;
    }

    public async Task<List<ServicePriceDto>> Handle(GetServicePricesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting service prices: ServiceId={ServiceId}, ServiceGarmentId={ServiceGarmentId}, IsActive={IsActive}",
            query.ServiceId, query.ServiceGarmentId, query.IsActive);

        var result = new List<ServicePriceDto>();

        if (query.ServiceId.HasValue)
        {
            var service = await _serviceRepository.GetByIdAsync(
                ServiceId.From(query.ServiceId.Value), cancellationToken);

            if (service != null)
            {
                var prices = await MapServicePrices(service, query, cancellationToken);
                result.AddRange(prices);
            }
        }
        else
        {
            var services = await _serviceRepository.GetAllAsync(cancellationToken);
            foreach (var service in services)
            {
                var prices = await MapServicePrices(service, query, cancellationToken);
                result.AddRange(prices);
            }
        }

        _logger.LogInformation("Found {Count} service prices", result.Count);
        return result;
    }

    private async Task<List<ServicePriceDto>> MapServicePrices(
        Domain.Aggregates.Services.ServicePure service,
        GetServicePricesQuery query,
        CancellationToken cancellationToken)
    {
        var result = new List<ServicePriceDto>();

        if (!service.IsPieceBased)
            return result;

        var prices = service.Prices.AsEnumerable();

        if (query.ServiceGarmentId.HasValue)
            prices = prices.Where(p => p.ServiceGarmentId.Value == query.ServiceGarmentId.Value);

        if (query.IsActive.HasValue)
            prices = prices.Where(p => p.IsActive == query.IsActive.Value);

        var priceList = prices.ToList();

        // Batch-fetch garment info
        var garmentIds = priceList.Select(p => p.ServiceGarmentId).Distinct();
        var garments = new Dictionary<int, Domain.Aggregates.ServiceGarments.ServiceGarmentPure>();

        foreach (var garmentId in garmentIds)
        {
            var garment = await _garmentRepository.GetByIdAsync(garmentId, cancellationToken);
            if (garment != null) garments[garmentId.Value] = garment;
        }

        foreach (var price in priceList)
        {
            ServiceGarmentDto? garmentDto = null;
            if (garments.TryGetValue(price.ServiceGarmentId.Value, out var garment))
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

            result.Add(new ServicePriceDto
            {
                Id = price.Id.Value,
                ServiceId = price.ServiceId.Value,
                GarmentTypeId = price.ServiceGarmentId.Value,
                GarmentType = garmentDto,
                UnitPrice = price.Price.Amount,
                IsActive = price.IsActive,
                CreatedAt = price.CreatedAt.ToString("o"),
                UpdatedAt = price.UpdatedAt?.ToString("o")
            });
        }

        return result;
    }
}
