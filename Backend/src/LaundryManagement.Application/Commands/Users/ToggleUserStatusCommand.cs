using LaundryManagement.Application.DTOs.Users;
using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public record ToggleUserStatusCommand(int Id) : IRequest<UserResponseDto>;
