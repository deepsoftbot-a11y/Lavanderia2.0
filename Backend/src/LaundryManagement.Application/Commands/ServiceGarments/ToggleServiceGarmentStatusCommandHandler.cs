using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Handler para activar/desactivar un tipo de prenda
/// </summary>
public sealed class ToggleServiceGarmentStatusCommandHandler : IRequestHandler<ToggleServiceGarmentStatusCommand, Unit>
{
    private readonly IServiceGarmentRepository _repository;
    private readonly ILogger<ToggleServiceGarmentStatusCommandHandler> _logger;

    public ToggleServiceGarmentStatusCommandHandler(
        IServiceGarmentRepository repository,
        ILogger<ToggleServiceGarmentStatusCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ToggleServiceGarmentStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Toggling service garment status: Id={GarmentId}, IsActive={IsActive}",
            command.ServiceGarmentId,
            command.IsActive
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

        // Activar o desactivar según el comando
        if (command.IsActive)
        {
            garment.Activate();
        }
        else
        {
            garment.Deactivate();
        }

        // Persistir cambios
        await _repository.UpdateAsync(garment, cancellationToken);

        _logger.LogInformation(
            "Service garment status toggled successfully: Id={GarmentId}, IsActive={IsActive}",
            command.ServiceGarmentId,
            command.IsActive
        );

        return Unit.Value;
    }
}
