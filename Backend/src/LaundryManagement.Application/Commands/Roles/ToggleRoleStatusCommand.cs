using LaundryManagement.Application.DTOs.Roles;
using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public record ToggleRoleStatusCommand(int Id) : IRequest<RoleResponseDto>;
