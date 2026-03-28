using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Handler para eliminar un tipo de prenda
/// </summary>
public sealed class DeleteServiceGarmentCommandHandler : IRequestHandler<DeleteServiceGarmentCommand, Unit>
{
    private readonly IServiceGarmentRepository _repository;
    private readonly ILogger<DeleteServiceGarmentCommandHandler> _logger;

    public DeleteServiceGarmentCommandHandler(
        IServiceGarmentRepository repository,
        ILogger<DeleteServiceGarmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteServiceGarmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting service garment: Id={GarmentId}",
            command.ServiceGarmentId
        );

        // Eliminar el tipo de prenda
        await _repository.DeleteAsync(
            ServiceGarmentId.From(command.ServiceGarmentId),
            cancellationToken
        );

        _logger.LogInformation(
            "Service garment deleted successfully: Id={GarmentId}",
            command.ServiceGarmentId
        );

        return Unit.Value;
    }
}
