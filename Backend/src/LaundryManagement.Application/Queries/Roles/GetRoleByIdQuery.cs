using LaundryManagement.Application.Commands.Roles;
using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Roles;

public record GetRoleByIdQuery(int Id) : IRequest<RoleResponseDto>;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleResponseDto>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponseDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(RoleId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Rol con ID {request.Id} no encontrado");

        var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(request.Id, cancellationToken);

        return new RoleResponseDto
        {
            Id = role.Id.Value,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Module = p.Module,
                Section = p.Section,
                Label = p.Label,
                Description = p.Description
            }).ToList()
        };
    }
}
