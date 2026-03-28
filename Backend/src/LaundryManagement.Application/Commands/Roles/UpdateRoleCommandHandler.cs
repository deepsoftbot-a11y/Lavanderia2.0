using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResponseDto>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponseDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(RoleId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Rol con ID {request.Id} no encontrado");

        if (request.Name != null)
        {
            if (await _roleRepository.ExistsAsync(request.Name, excludeId: request.Id, cancellationToken: cancellationToken))
                throw new BusinessRuleException($"Ya existe un rol con el nombre '{request.Name}'");

            role.Update(request.Name, request.Description ?? role.Description);
        }
        else if (request.Description != null)
        {
            role.Update(role.Name, request.Description);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value && !role.IsActive)
                role.Activate();
            else if (!request.IsActive.Value && role.IsActive)
                role.Deactivate();
        }

        if (request.PermissionIds != null)
            role.SetPermissions(request.PermissionIds);

        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return CreateRoleCommandHandler.MapToDto(role);
    }
}
