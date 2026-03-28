namespace LaundryManagement.Application.DTOs.Roles;

public sealed class RoleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<PermissionDto>? Permissions { get; set; }
}
