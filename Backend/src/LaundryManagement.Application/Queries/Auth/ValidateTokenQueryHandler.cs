using LaundryManagement.Application.DTOs.Auth;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Auth;

/// <summary>
/// Handler for ValidateTokenQuery
/// </summary>
public sealed class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, ValidateTokenResponseDto>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;

    public ValidateTokenQueryHandler(
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    public async Task<ValidateTokenResponseDto> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return new ValidateTokenResponseDto
                {
                    Success = false,
                    Message = "Token no proporcionado"
                };
            }

            // Validate token and extract user ID
            var userId = _jwtTokenService.ValidateToken(request.Token);

            if (userId == null)
            {
                return new ValidateTokenResponseDto
                {
                    Success = false,
                    Message = "Token inválido o expirado"
                };
            }

            // Get user by ID
            var user = await _userRepository.GetByIdAsync(UserId.From(userId.Value), cancellationToken);

            if (user == null)
            {
                return new ValidateTokenResponseDto
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                };
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return new ValidateTokenResponseDto
                {
                    Success = false,
                    Message = "Usuario inactivo"
                };
            }

            // Map to UserDto
            var userDto = new UserDto
            {
                Id = user.Id.Value,
                Username = user.Username.Value,
                Name = user.FullName,
                Email = user.Email.Value,
                Role = user.PrimaryRole?.Value ?? "empleado",
                Status = user.IsActive ? "active" : "inactive",
                Permissions = user.Permissions.Select(p => p.PermissionName).ToList(),
                CreatedAt = user.CreatedAt.ToString("O"), // ISO8601
                CreatedBy = user.CreatedBy,
                LastLogin = user.LastLogin?.ToString("O") // ISO8601
            };

            return new ValidateTokenResponseDto
            {
                Success = true,
                User = userDto,
                Token = request.Token
            };
        }
        catch (Exception)
        {
            // Log the exception here if you have a logger
            return new ValidateTokenResponseDto
            {
                Success = false,
                Message = "Error al validar el token"
            };
        }
    }
}
