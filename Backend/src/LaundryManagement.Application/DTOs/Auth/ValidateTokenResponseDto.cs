namespace LaundryManagement.Application.DTOs.Auth;

/// <summary>
/// DTO for token validation response
/// </summary>
public sealed class ValidateTokenResponseDto
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
}
