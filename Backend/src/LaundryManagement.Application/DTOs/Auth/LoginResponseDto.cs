namespace LaundryManagement.Application.DTOs.Auth;

/// <summary>
/// DTO for login response
/// </summary>
public sealed class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public string? Message { get; set; }
}
