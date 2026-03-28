using LaundryManagement.Application.DTOs.Roles;
using MediatR;

namespace LaundryManagement.Application.Commands.Permissions;

public record UpdatePermissionCommand(
    int Id,
    string? Name,
    string? Module,
    string? Description
) : IRequest<PermissionDto>;
