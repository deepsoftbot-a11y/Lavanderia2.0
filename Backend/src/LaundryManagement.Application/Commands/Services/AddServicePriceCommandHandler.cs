using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Services;

public sealed class AddServicePriceCommandHandler : IRequestHandler<AddServicePriceCommand, int>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<AddServicePriceCommandHandler> _logger;

    public AddServicePriceCommandHandler(
        IServiceRepository serviceRepository,
        ILogger<AddServicePriceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<int> Handle(AddServicePriceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Agregando precio al servicio {ServiceId} para prenda {ServiceGarmentId}",
                command.ServiceId, command.ServiceGarmentId);

            var service = await _serviceRepository.GetByIdAsync(ServiceId.From(command.ServiceId), cancellationToken);
            if (service == null)
                throw new NotFoundException($"Servicio con ID {command.ServiceId} no encontrado");

            service.AddPriceForGarment(
                ServiceGarmentId.From(command.ServiceGarmentId),
                Money.FromDecimal(command.Price)
            );

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            _logger.LogInformation("Precio agregado exitosamente");

            // Retornar el ID del precio agregado (el último en la colección)
            var addedPrice = service.Prices.Last();
            return addedPrice.Id.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar precio al servicio {ServiceId}", command.ServiceId);
            throw;
        }
    }
}
