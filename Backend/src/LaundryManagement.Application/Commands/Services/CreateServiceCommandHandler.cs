using LaundryManagement.Application.DTOs.Services;
using LaundryManagement.Domain.Aggregates.Services;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Services;

/// <summary>
/// Handler que implementa el patrón DDD para crear servicios.
/// </summary>
public sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CreateServiceCommandHandler> _logger;

    public CreateServiceCommandHandler(
        IServiceRepository serviceRepository,
        ICategoryRepository categoryRepository,
        ILogger<CreateServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<ServiceDto> Handle(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creando servicio {Code}", command.Code);

            // Obtener la categoría para crear la referencia
            var categoryId = CategoryId.From(command.CategoryId);
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
                ?? throw new NotFoundException($"Categoría con ID {command.CategoryId} no encontrada");

            var categoryReference = CategoryReference.Create(categoryId, category.Name);

            // Crear el agregado según el tipo de unidad
            ServicePure service;
            var chargeType = command.ChargeType.ToLowerInvariant();

            if (chargeType == "piece")
            {
                service = ServicePure.CreatePieceBased(
                    code: ServiceCode.From(command.Code),
                    name: command.Name,
                    category: categoryReference,
                    basePrice: null,
                    description: command.Description,
                    icon: command.Icon,
                    estimatedHours: command.EstimatedTime
                );
            }
            else if (chargeType == "kg" || chargeType == "weight")
            {
                var weightPricing = WeightPricing.Create(
                    pricePerKilo: Money.FromDecimal(command.PricePerKg ?? 0),
                    minimumWeight: command.MinWeight,
                    maximumWeight: command.MaxWeight
                );

                service = ServicePure.CreateWeightBased(
                    code: ServiceCode.From(command.Code),
                    name: command.Name,
                    category: categoryReference,
                    weightPricing: weightPricing,
                    description: command.Description,
                    icon: command.Icon,
                    estimatedHours: command.EstimatedTime
                );
            }
            else
            {
                throw new ArgumentException($"Tipo de cobro no válido: {command.ChargeType}. Use 'piece' o 'kg'.");
            }

            // Persistir el servicio
            var savedService = await _serviceRepository.AddAsync(service, cancellationToken);

            _logger.LogInformation("Servicio creado exitosamente: {ServiceId}", savedService.Id.Value);

            return MapToDto(savedService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear servicio {Code}", command.Code);
            throw;
        }
    }

    internal static ServiceDto MapToDto(ServicePure service) => new()
    {
        Id = service.Id.Value,
        Code = service.Code.Value,
        Name = service.Name,
        Description = service.Description,
        CategoryId = service.CategoryId.Value,
        Category = new CategoryDto { Id = service.Category.Id.Value, Name = service.Category.Name ?? string.Empty },
        ChargeType = service.UnitType.IsPiece ? "piece" : "kg",
        PricePerKg = service.BasePrice?.Amount ?? service.WeightPricing?.PricePerKilo.Amount ?? 0,
        MinWeight = service.WeightPricing?.MinimumWeight,
        MaxWeight = service.WeightPricing?.MaximumWeight,
        IsActive = service.IsActive,
        Icon = service.Icon,
        EstimatedTime = service.EstimatedHours,
        CreatedAt = service.CreatedAt.ToString("o"),
        UpdatedAt = service.UpdatedAt?.ToString("o")
    };
}
