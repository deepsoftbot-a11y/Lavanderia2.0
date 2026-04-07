using LaundryManagement.Application.DTOs.Roles;
using MediatR;

namespace LaundryManagement.Application.Commands.Permissions;

public record UpdatePermissionCommand(
    int Id,
    string? Name,
    string? Module,
    string? Section,
    string? Label,
    string? Description
) : IRequest<PermissionDto>;
