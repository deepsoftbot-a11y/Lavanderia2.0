using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Handler para actualizar un tipo de prenda existente
/// </summary>
public sealed class UpdateServiceGarmentCommandHandler : IRequestHandler<UpdateServiceGarmentCommand, Unit>
{
    private readonly IServiceGarmentRepository _repository;
    private readonly ILogger<UpdateServiceGarmentCommandHandler> _logger;

    public UpdateServiceGarmentCommandHandler(
        IServiceGarmentRepository repository,
        ILogger<UpdateServiceGarmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateServiceGarmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating service garment: Id={GarmentId}, Name={Name}",
            command.ServiceGarmentId,
            command.Name
        );

        // Obtener el agregado
        var garment = await _repository.GetByIdAsync(
            ServiceGarmentId.From(command.ServiceGarmentId),
            cancellationToken
        );

        if (garment == null)
        {
            throw new NotFoundException(
                $"Tipo de prenda con ID {command.ServiceGarmentId} no encontrado"
            );
        }

        // Actualizar la información a través del método de dominio
        garment.UpdateInfo(
            name: command.Name,
            description: command.Description
        );

        // Actualizar estado activo/inactivo si se especificó
        if (command.IsActive.HasValue)
        {
            if (command.IsActive.Value) garment.Activate();
            else garment.Deactivate();
        }

        // Persistir cambios
        await _repository.UpdateAsync(garment, cancellationToken);

        _logger.LogInformation(
            "Service garment updated successfully: Id={GarmentId}",
            command.ServiceGarmentId
        );

        return Unit.Value;
    }
}
