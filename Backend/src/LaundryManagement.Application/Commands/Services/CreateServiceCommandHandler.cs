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
            // Resolver código: usar el provisto o auto-generar desde el nombre
            var serviceCode = string.IsNullOrWhiteSpace(command.Code)
                ? await ResolveCodeAsync(command.Name, cancellationToken)
                : command.Code;

            _logger.LogInformation("Creando servicio {Code}", serviceCode);

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
                    code: ServiceCode.From(serviceCode),
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
                    code: ServiceCode.From(serviceCode),
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

            // Persistir el servicio (necesitamos el ID antes de agregar precios)
            var savedService = await _serviceRepository.AddAsync(service, cancellationToken);

            _logger.LogInformation("Servicio creado exitosamente: {ServiceId}", savedService.Id.Value);

            // Agregar precios en batch DESPUÉS de persistir (el ID ya está asignado)
            if (chargeType == "piece" && command.GarmentPrices != null && command.GarmentPrices.Count > 0)
            {
                foreach (var gp in command.GarmentPrices)
                {
                    savedService.AddPriceForGarment(
                        ServiceGarmentId.From(gp.GarmentTypeId),
                        Money.FromDecimal(gp.UnitPrice)
                    );
                }
                await _serviceRepository.UpdateAsync(savedService, cancellationToken);
            }

            return MapToDto(savedService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear servicio {Code}", command.Code);
            throw;
        }
    }

    private static string GenerateCodePrefix(string name)
    {
        var letters = name.Trim()
            .ToUpperInvariant()
            .Where(char.IsAsciiLetter)
            .Take(3)
            .ToArray();
        return new string(letters).PadRight(3, 'X');
    }

    private async Task<string> ResolveCodeAsync(string name, CancellationToken ct)
    {
        var prefix = GenerateCodePrefix(name);
        for (int seq = 1; seq <= 999; seq++)
        {
            var candidate = $"{prefix}-{seq:D3}";
            var existing = await _serviceRepository.GetByCodeAsync(ServiceCode.From(candidate), ct);
            if (existing == null)
                return candidate;
        }
        throw new Domain.Exceptions.BusinessRuleException(
            $"No se pudo generar un código único para el prefijo '{prefix}'");
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
