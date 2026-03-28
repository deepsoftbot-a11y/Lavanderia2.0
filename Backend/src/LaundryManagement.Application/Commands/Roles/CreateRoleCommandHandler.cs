using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Aggregates.Roles;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResponseDto>
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponseDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _roleRepository.ExistsAsync(request.Name, cancellationToken: cancellationToken))
            throw new BusinessRuleException($"Ya existe un rol con el nombre '{request.Name}'");

        var role = RolePure.Create(request.Name, request.Description, request.IsActive, request.PermissionIds);
        await _roleRepository.AddAsync(role, cancellationToken);

        return MapToDto(role);
    }

    internal static RoleResponseDto MapToDto(RolePure role, bool includePermissions = false, IEnumerable<PermissionDto>? permissions = null)
    {
        return new RoleResponseDto
        {
            Id = role.Id.Value,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = includePermissions ? permissions?.ToList() : null
        };
    }
}
