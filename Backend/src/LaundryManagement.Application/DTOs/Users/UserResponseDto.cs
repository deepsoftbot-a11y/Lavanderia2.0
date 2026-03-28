namespace LaundryManagement.Application.DTOs.Users;

public sealed class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public UserRoleDto? Role { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? LastLogin { get; set; }
    public int? CreatedBy { get; set; }
}
