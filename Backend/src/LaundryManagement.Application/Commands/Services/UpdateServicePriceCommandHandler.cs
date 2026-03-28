using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Services;

public sealed class UpdateServicePriceCommandHandler : IRequestHandler<UpdateServicePriceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<UpdateServicePriceCommandHandler> _logger;

    public UpdateServicePriceCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<UpdateServicePriceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateServicePriceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Actualizando precio {ServicePriceId}", command.ServicePriceId);

            // Buscar el servicio que contiene el precio con el ID dado
            var services = await _serviceRepository.GetAllAsync(cancellationToken);

            var targetService = services.FirstOrDefault(
                s => s.Prices.Any(p => p.Id.Value == command.ServicePriceId));

            if (targetService == null)
                throw new NotFoundException($"Precio con ID {command.ServicePriceId} no encontrado");

            targetService.UpdatePriceById(
                ServicePriceId.From(command.ServicePriceId),
                Money.FromDecimal(command.UnitPrice)
            );

            await _serviceRepository.UpdateAsync(targetService, cancellationToken);
            _logger.LogInformation("Precio actualizado exitosamente");

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio {ServicePriceId}", command.ServicePriceId);
            throw;
        }
    }
}
