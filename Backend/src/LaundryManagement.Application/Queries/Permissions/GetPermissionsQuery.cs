using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Permissions;

public record GetPermissionsQuery : IRequest<List<PermissionDto>>;

public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
{
    private readonly IRoleRepository _roleRepository;

    public GetPermissionsQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _roleRepository.GetAllPermissionsAsync(cancellationToken);

        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Module = p.Module,
            Description = p.Description
        }).ToList();
    }
}
