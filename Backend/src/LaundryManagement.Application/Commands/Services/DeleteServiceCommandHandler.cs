using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Services;

public sealed class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<DeleteServiceCommandHandler> _logger;

    public DeleteServiceCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<DeleteServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteServiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Eliminando servicio {ServiceId}", command.ServiceId);

            await _serviceRepository.DeleteAsync(ServiceId.From(command.ServiceId), cancellationToken);

            _logger.LogInformation("Servicio eliminado exitosamente");
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar servicio {ServiceId}", command.ServiceId);
            throw;
        }
    }
}
