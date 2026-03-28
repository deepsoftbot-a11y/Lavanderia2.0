using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Commands.Permissions;

public sealed class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, PermissionDto>
{
    private readonly IPermissionRepository _permissionRepository;

    public UpdatePermissionCommandHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<PermissionDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Permiso con ID {request.Id} no encontrado");

        if (request.Name != null && request.Name != existing.Name)
        {
            if (await _permissionRepository.ExistsAsync(request.Name, excludeId: request.Id, cancellationToken: cancellationToken))
                throw new BusinessRuleException($"Ya existe un permiso con el nombre '{request.Name}'");
        }

        var updated = await _permissionRepository.UpdateAsync(
            request.Id, request.Name, request.Module, request.Description, cancellationToken);

        return new PermissionDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Module = updated.Module,
            Description = updated.Description
        };
    }
}
