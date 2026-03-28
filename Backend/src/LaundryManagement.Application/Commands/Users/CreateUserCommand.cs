using LaundryManagement.Application.DTOs.Users;
using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public record CreateUserCommand(
    string Username,
    string FullName,
    string Email,
    string Password,
    int? RoleId,
    bool IsActive
) : IRequest<UserResponseDto>;
