using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Services;

public sealed class ToggleServiceStatusCommandHandler : IRequestHandler<ToggleServiceStatusCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ToggleServiceStatusCommandHandler> _logger;

    public ToggleServiceStatusCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<ToggleServiceStatusCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ToggleServiceStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cambiando estado de servicio {ServiceId} a {IsActive}",
                command.ServiceId, command.IsActive);

            var service = await _serviceRepository.GetByIdAsync(ServiceId.From(command.ServiceId), cancellationToken);
            if (service == null)
                throw new NotFoundException($"Servicio con ID {command.ServiceId} no encontrado");

            if (command.IsActive)
                service.Activate();
            else
                service.Deactivate();

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            _logger.LogInformation("Estado del servicio cambiado exitosamente");

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de servicio {ServiceId}", command.ServiceId);
            throw;
        }
    }
}
