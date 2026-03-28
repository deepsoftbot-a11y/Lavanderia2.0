using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Entities;

/// <summary>
/// Entity representing a role assignment to a user
/// </summary>
public sealed class UserRoleAssignment : Entity<int>
{
    public int RoleId { get; private set; }
    public UserRole Role { get; private set; } = null!;
    public DateTime AssignedAt { get; private set; }

    private UserRoleAssignment()
    {
    }

    /// <summary>
    /// Factory method for creating new role assignments
    /// </summary>
    internal static UserRoleAssignment Create(int roleId, UserRole role, DateTime assignedAt)
    {
        return new UserRoleAssignment
        {
            Id = 0,
            RoleId = roleId,
            Role = role,
            AssignedAt = assignedAt
        };
    }

    /// <summary>
    /// Factory method for reconstituting from database (internal for repository use)
    /// </summary>
    internal static UserRoleAssignment Reconstitute(int id, int roleId, UserRole role, DateTime assignedAt)
    {
        return new UserRoleAssignment
        {
            Id = id,
            RoleId = roleId,
            Role = role,
            AssignedAt = assignedAt
        };
    }
}
