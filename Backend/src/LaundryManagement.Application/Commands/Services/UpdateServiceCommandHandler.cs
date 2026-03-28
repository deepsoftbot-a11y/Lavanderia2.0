using LaundryManagement.Application.DTOs.Services;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Services;

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceDto>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<UpdateServiceCommandHandler> _logger;

    public UpdateServiceCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<UpdateServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<ServiceDto> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Actualizando servicio {ServiceId}", command.ServiceId);

            var service = await _serviceRepository.GetByIdAsync(ServiceId.From(command.ServiceId), cancellationToken);
            if (service == null)
                throw new NotFoundException($"Servicio con ID {command.ServiceId} no encontrado");

            // Actualizar información básica
            service.UpdateInfo(
                name: command.Name,
                description: command.Description,
                icon: command.Icon,
                estimatedHours: command.EstimatedTime
            );

            // Actualizar precios según el tipo de servicio
            if (service.IsPieceBased && command.PricePerKg.HasValue)
            {
                service.UpdateBasePrice(Money.FromDecimal(command.PricePerKg.Value));
            }
            else if (service.IsWeightBased && command.PricePerKg.HasValue)
            {
                var weightPricing = WeightPricing.Create(
                    Money.FromDecimal(command.PricePerKg.Value),
                    command.MinWeight,
                    command.MaxWeight
                );
                service.UpdateWeightPricing(weightPricing);
            }

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            _logger.LogInformation("Servicio actualizado exitosamente");

            return CreateServiceCommandHandler.MapToDto(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar servicio {ServiceId}", command.ServiceId);
            throw;
        }
    }
}
