using LaundryManagement.Application.DTOs.Roles;
using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public record UpdateRoleCommand(
    int Id,
    string? Name,
    string? Description,
    bool? IsActive,
    List<int>? PermissionIds
) : IRequest<RoleResponseDto>;
