using LaundryManagement.Application.DTOs.Auth;
using MediatR;

namespace LaundryManagement.Application.Commands.Auth;

/// <summary>
/// Command for user authentication
/// </summary>
public sealed record LoginCommand(
    string Username,
    string Password
) : IRequest<LoginResponseDto>;
