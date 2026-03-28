using LaundryManagement.Application.DTOs.Auth;
using MediatR;

namespace LaundryManagement.Application.Queries.Auth;

/// <summary>
/// Query for validating a JWT token
/// </summary>
public sealed record ValidateTokenQuery(
    string Token
) : IRequest<ValidateTokenResponseDto>;
