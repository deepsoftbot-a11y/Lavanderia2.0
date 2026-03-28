using LaundryManagement.Domain.Aggregates.ServiceGarments;
using LaundryManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Handler que implementa el patrón DDD para crear tipos de prenda.
/// Usa el agregado ServiceGarmentPure y el repositorio de dominio.
/// </summary>
public sealed class CreateServiceGarmentCommandHandler : IRequestHandler<CreateServiceGarmentCommand, int>
{
    private readonly IServiceGarmentRepository _repository;
    private readonly ILogger<CreateServiceGarmentCommandHandler> _logger;

    public CreateServiceGarmentCommandHandler(
        IServiceGarmentRepository repository,
        ILogger<CreateServiceGarmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> Handle(CreateServiceGarmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating new service garment: Name={Name}",
            command.Name
        );

        // Crear el agregado usando el factory method
        var garment = ServiceGarmentPure.Create(
            name: command.Name,
            description: command.Description
        );

        // Persistir el agregado
        var savedGarment = await _repository.AddAsync(garment, cancellationToken);

        _logger.LogInformation(
            "Service garment created successfully: Id={GarmentId}, Name={Name}",
            savedGarment.Id.Value,
            savedGarment.Name
        );

        return savedGarment.Id.Value;
    }
}
