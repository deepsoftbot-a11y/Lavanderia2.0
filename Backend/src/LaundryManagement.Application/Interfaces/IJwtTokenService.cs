using LaundryManagement.Application.DTOs.Auth;

namespace LaundryManagement.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for an authenticated user
    /// </summary>
    string GenerateToken(AuthenticatedUserDto user);

    /// <summary>
    /// Validates a JWT token and returns the user ID if valid
    /// </summary>
    int? ValidateToken(string token);

    /// <summary>
    /// Extracts the user ID from a token without full validation (for logging)
    /// </summary>
    int? ExtractUserIdFromToken(string token);
}
