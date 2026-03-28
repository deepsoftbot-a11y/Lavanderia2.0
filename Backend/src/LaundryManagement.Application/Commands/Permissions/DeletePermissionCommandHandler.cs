using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Commands.Permissions;

public sealed class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand>
{
    private readonly IPermissionRepository _permissionRepository;

    public DeletePermissionCommandHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Permiso con ID {request.Id} no encontrado");

        if (await _permissionRepository.IsAssignedToRolesAsync(request.Id, cancellationToken))
            throw new BusinessRuleException(
                $"El permiso '{existing.Name}' está asignado a uno o más roles. Desasígnalo primero.");

        await _permissionRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
