using LaundryManagement.Application.DTOs.Auth;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Auth;

/// <summary>
/// Handler for LoginCommand
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario y contraseña son requeridos"
                };
            }

            // Parse username
            Username username;
            try
            {
                username = Username.From(request.Username);
            }
            catch
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Credenciales inválidas"
                };
            }

            // Get user by username
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Credenciales inválidas"
                };
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Usuario inactivo. Contacte al administrador."
                };
            }

            // Verify password
            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Value);

            if (!isPasswordValid)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Credenciales inválidas"
                };
            }

            // Update last login
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Create authenticated user DTO
            var authenticatedUser = new AuthenticatedUserDto
            {
                UserId = user.Id.Value,
                Username = user.Username.Value,
                Email = user.Email.Value,
                FullName = user.FullName,
                Role = user.PrimaryRole?.Value ?? "empleado",
                Permissions = user.Permissions.Select(p => p.PermissionName).ToList()
            };

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(authenticatedUser);

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

            return new LoginResponseDto
            {
                Success = true,
                Token = token,
                User = userDto
            };
        }
        catch (Exception ex)
        {
            // Log the exception here if you have a logger
            return new LoginResponseDto
            {
                Success = false,
                Message = "Error interno del servidor. Intente nuevamente."
            };
        }
    }
}
