using LaundryManagement.Application.DTOs.Auth;
using MediatR;

namespace LaundryManagement.Application.Queries.Auth;

/// <summary>
/// Query for getting a user by ID
/// </summary>
public sealed record GetUserByIdQuery(
    int UserId
) : IRequest<UserDto?>;
