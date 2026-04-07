using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServicePrices;

public sealed class ToggleServicePriceStatusCommandHandler : IRequestHandler<ToggleServicePriceStatusCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ToggleServicePriceStatusCommandHandler> _logger;

    public ToggleServicePriceStatusCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<ToggleServicePriceStatusCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ToggleServicePriceStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toggling service price status: ID={ServicePriceId}", command.ServicePriceId);

        // Obtener el ServicioId directamente sin hacer full table scan
        var priceInfo = await _serviceRepository.GetServicePriceByIdAsync(command.ServicePriceId, cancellationToken);
        if (priceInfo == null)
            throw new NotFoundException($"Precio con ID {command.ServicePriceId} no encontrado");

        // Cargar SOLO el servicio padre, no todos los servicios
        var targetService = await _serviceRepository.GetByIdAsync(ServiceId.From(priceInfo.Value.ServicioId), cancellationToken);
        if (targetService == null)
            throw new NotFoundException($"Servicio con ID {priceInfo.Value.ServicioId} no encontrado");

        targetService.TogglePriceStatusById(ServicePriceId.From(command.ServicePriceId));
        await _serviceRepository.UpdateAsync(targetService, cancellationToken);

        _logger.LogInformation("Service price status toggled: ID={ServicePriceId}", command.ServicePriceId);
        return Unit.Value;
    }
}
