using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Application.DTOs.ServicePrices;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServicePrices;

public sealed class CreateServicePriceCommandHandler : IRequestHandler<CreateServicePriceCommand, ServicePriceDto>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceGarmentRepository _garmentRepository;
    private readonly ILogger<CreateServicePriceCommandHandler> _logger;

    public CreateServicePriceCommandHandler(
        IServiceRepository serviceRepository,
        IServiceGarmentRepository garmentRepository,
        ILogger<CreateServicePriceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _garmentRepository = garmentRepository;
        _logger = logger;
    }

    public async Task<ServicePriceDto> Handle(CreateServicePriceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding service price: ServiceId={ServiceId}, GarmentTypeId={GarmentTypeId}",
            command.ServiceId, command.GarmentTypeId);

        var service = await _serviceRepository.GetByIdAsync(ServiceId.From(command.ServiceId), cancellationToken)
            ?? throw new NotFoundException($"Servicio con ID {command.ServiceId} no encontrado");

        service.AddPriceForGarment(
            ServiceGarmentId.From(command.GarmentTypeId),
            Money.FromDecimal(command.UnitPrice)
        );

        await _serviceRepository.UpdateAsync(service, cancellationToken);

        // Re-query to get the saved price with DB-assigned ID
        var updatedService = await _serviceRepository.GetByIdAsync(ServiceId.From(command.ServiceId), cancellationToken);
        var newPrice = updatedService!.Prices
            .Where(p => p.ServiceGarmentId.Value == command.GarmentTypeId && p.IsActive)
            .MaxBy(p => p.Id.Value);

        // Fetch garment info
        var garment = await _garmentRepository.GetByIdAsync(
            ServiceGarmentId.From(command.GarmentTypeId), cancellationToken);

        ServiceGarmentDto? garmentDto = garment != null ? new ServiceGarmentDto
        {
            Id = garment.Id.Value,
            Name = garment.Name,
            Description = garment.Description,
            IsActive = garment.IsActive,
            CreatedAt = garment.CreatedAt.ToString("o"),
            UpdatedAt = garment.UpdatedAt?.ToString("o")
        } : null;

        return new ServicePriceDto
        {
            Id = newPrice?.Id.Value ?? 0,
            ServiceId = command.ServiceId,
            GarmentTypeId = command.GarmentTypeId,
            GarmentType = garmentDto,
            UnitPrice = command.UnitPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = null
        };
    }
}
