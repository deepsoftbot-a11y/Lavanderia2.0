namespace LaundryManagement.Application.DTOs.Auth;

/// <summary>
/// DTO for internal use when generating JWT tokens
/// </summary>
public sealed class AuthenticatedUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
