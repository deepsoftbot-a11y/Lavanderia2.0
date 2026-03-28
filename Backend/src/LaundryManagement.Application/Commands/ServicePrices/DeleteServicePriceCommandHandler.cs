using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServicePrices;

public sealed class DeleteServicePriceCommandHandler : IRequestHandler<DeleteServicePriceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<DeleteServicePriceCommandHandler> _logger;

    public DeleteServicePriceCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<DeleteServicePriceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteServicePriceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting service price: ID={ServicePriceId}", command.ServicePriceId);

        var services = await _serviceRepository.GetAllAsync(cancellationToken);

        var targetService = services.FirstOrDefault(
            s => s.Prices.Any(p => p.Id.Value == command.ServicePriceId));

        if (targetService == null)
            throw new NotFoundException($"Precio con ID {command.ServicePriceId} no encontrado");

        targetService.DeactivatePriceById(ServicePriceId.From(command.ServicePriceId));
        await _serviceRepository.UpdateAsync(targetService, cancellationToken);

        _logger.LogInformation("Service price deactivated: ID={ServicePriceId}", command.ServicePriceId);
        return Unit.Value;
    }
}
