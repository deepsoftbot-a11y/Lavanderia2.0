namespace LaundryManagement.Application.DTOs.Auth;

/// <summary>
/// DTO for user information returned to the frontend
/// </summary>
public sealed class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty; // ISO8601 format
    public int? CreatedBy { get; set; }
    public string? LastLogin { get; set; } // ISO8601 format
}
