using LaundryManagement.Application.DTOs.Users;
using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public record UpdateUserCommand(
    int Id,
    string? FullName,
    string? Email,
    int? RoleId,        // null = remove role, value = assign that role (always processed in PUT)
    bool? IsActive,
    string? Password
) : IRequest<UserResponseDto>;
