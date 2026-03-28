using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public sealed class ToggleRoleStatusCommandHandler : IRequestHandler<ToggleRoleStatusCommand, RoleResponseDto>
{
    private readonly IRoleRepository _roleRepository;

    public ToggleRoleStatusCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponseDto> Handle(ToggleRoleStatusCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(RoleId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Rol con ID {request.Id} no encontrado");

        if (role.IsActive)
            role.Deactivate();
        else
            role.Activate();

        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return CreateRoleCommandHandler.MapToDto(role);
    }
}
