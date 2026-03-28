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

        var services = await _serviceRepository.GetAllAsync(cancellationToken);

        var targetService = services.FirstOrDefault(
            s => s.Prices.Any(p => p.Id.Value == command.ServicePriceId));

        if (targetService == null)
            throw new NotFoundException($"Precio con ID {command.ServicePriceId} no encontrado");

        targetService.TogglePriceStatusById(ServicePriceId.From(command.ServicePriceId));
        await _serviceRepository.UpdateAsync(targetService, cancellationToken);

        _logger.LogInformation("Service price status toggled: ID={ServicePriceId}", command.ServicePriceId);
        return Unit.Value;
    }
}
