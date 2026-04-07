using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.Entities;

/// <summary>
/// Entity representing a permission assigned to a user (through roles)
/// </summary>
public sealed class UserPermission : Entity<int>
{
    public int PermissionId { get; private set; }
    public string PermissionName { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Section { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;

    private UserPermission()
    {
    }

    /// <summary>
    /// Factory method for reconstituting from database (internal for repository use)
    /// </summary>
    internal static UserPermission Reconstitute(int id, int permissionId, string permissionName, string module, string section, string label)
    {
        return new UserPermission
        {
            Id = id,
            PermissionId = permissionId,
            PermissionName = permissionName,
            Module = module,
            Section = section,
            Label = label
        };
    }
}
