using LaundryManagement.Application.Commands.Roles;
using LaundryManagement.Application.DTOs.Roles;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Roles;

public record GetRolesQuery(bool? IsActive = null) : IRequest<List<RoleResponseDto>>;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleResponseDto>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<RoleResponseDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(request.IsActive, cancellationToken);
        return roles.Select(r => CreateRoleCommandHandler.MapToDto(r)).ToList();
    }
}
