using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Commands.Permissions;

public sealed class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, PermissionDto>
{
    private readonly IPermissionRepository _permissionRepository;

    public CreatePermissionCommandHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<PermissionDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        if (await _permissionRepository.ExistsAsync(request.Name, cancellationToken: cancellationToken))
            throw new BusinessRuleException($"Ya existe un permiso con el nombre '{request.Name}'");

        var permission = await _permissionRepository.AddAsync(
            request.Name, request.Module, request.Description, cancellationToken);

        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Module = permission.Module,
            Description = permission.Description
        };
    }
}
