using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.DomainEvents.Users;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Users;

/// <summary>
/// Pure Domain Aggregate Root representing a User in the authentication system
/// Encapsulates all business rules and invariants for user management
/// </summary>
public sealed class UserPure : AggregateRoot<UserId>
{
    private readonly List<UserRoleAssignment> _roleAssignments = new();
    private readonly List<UserPermission> _permissions = new();

    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public int? CreatedBy { get; private set; }

    public IReadOnlyCollection<UserRoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();
    public IReadOnlyCollection<UserPermission> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Gets the primary role of the user (first role assignment)
    /// </summary>
    public UserRole? PrimaryRole => _roleAssignments.FirstOrDefault()?.Role;

    private UserPure()
    {
        Username = Username.From("default");
        Email = Email.From("default@example.com");
        PasswordHash = PasswordHash.FromHash("default");
        FullName = string.Empty;
    }

    /// <summary>
    /// Factory method for creating a new user
    /// </summary>
    public static UserPure Create(
        Username username,
        Email email,
        PasswordHash passwordHash,
        string fullName,
        int? createdBy = null)
    {
        ValidateFullName(fullName);

        var user = new UserPure
        {
            Id = UserId.Empty(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLogin = null,
            CreatedBy = createdBy
        };

        user.RaiseDomainEvent(new UserCreated(
            0, // Will be set after persistence
            username.Value,
            email.Value
        ));

        return user;
    }

    /// <summary>
    /// Factory method for reconstituting user from database (internal for repository use)
    /// </summary>
    internal static UserPure Reconstitute(
        UserId id,
        Username username,
        Email email,
        PasswordHash passwordHash,
        string fullName,
        bool isActive,
        DateTime createdAt,
        DateTime? lastLogin,
        int? createdBy,
        List<UserRoleAssignment> roleAssignments,
        List<UserPermission> permissions)
    {
        var user = new UserPure
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            IsActive = isActive,
            CreatedAt = createdAt,
            LastLogin = lastLogin,
            CreatedBy = createdBy
        };

        user._roleAssignments.AddRange(roleAssignments);
        user._permissions.AddRange(permissions);

        return user;
    }

    /// <summary>
    /// Updates the last login timestamp
    /// </summary>
    public void UpdateLastLogin()
    {
        if (!IsActive)
        {
            throw new BusinessRuleException("No se puede actualizar el último acceso de un usuario inactivo");
        }

        LastLogin = DateTime.UtcNow;
        RaiseDomainEvent(new UserLoggedIn(Id.Value, Username.Value));
    }

    /// <summary>
    /// Checks if the user has a specific role
    /// </summary>
    public bool HasRole(string roleName)
    {
        return _roleAssignments.Any(ra => ra.Role.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the user has a specific permission
    /// </summary>
    public bool HasPermission(string permissionName)
    {
        return _permissions.Any(p => p.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Deactivates the user
    /// </summary>
    public void Deactivate(int deactivatedBy)
    {
        if (!IsActive)
        {
            throw new BusinessRuleException("El usuario ya está inactivo");
        }

        IsActive = false;
        RaiseDomainEvent(new UserDeactivated(Id.Value, deactivatedBy));
    }

    /// <summary>
    /// Activates the user
    /// </summary>
    public void Activate(int activatedBy)
    {
        if (IsActive)
        {
            throw new BusinessRuleException("El usuario ya está activo");
        }

        IsActive = true;
        RaiseDomainEvent(new UserActivated(Id.Value, activatedBy));
    }

    /// <summary>
    /// Changes the user's password
    /// </summary>
    public void ChangePassword(PasswordHash newPasswordHash, int changedBy)
    {
        if (!IsActive)
        {
            throw new BusinessRuleException("No se puede cambiar la contraseña de un usuario inactivo");
        }

        PasswordHash = newPasswordHash;
        RaiseDomainEvent(new UserPasswordChanged(Id.Value, changedBy));
    }

    /// <summary>
    /// Updates the user's full name and email
    /// </summary>
    public void UpdateDetails(string fullName, Email email)
    {
        ValidateFullName(fullName);
        FullName = fullName;
        Email = email;
    }

    /// <summary>
    /// Assigns a role to the user, replacing any existing role assignment
    /// </summary>
    public void AssignRole(int roleId, string roleName)
    {
        _roleAssignments.Clear();
        var role = UserRole.From(roleName);
        _roleAssignments.Add(UserRoleAssignment.Create(roleId, role, DateTime.UtcNow));
    }

    /// <summary>
    /// Removes all role assignments from the user
    /// </summary>
    public void RemoveAllRoles()
    {
        _roleAssignments.Clear();
    }

    /// <summary>
    /// Sets the ID after persistence (internal for repository use)
    /// </summary>
    internal void SetId(UserId id)
    {
        Id = id;
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ValidationException("El nombre completo no puede estar vacío");
        }

        if (fullName.Length > 200)
        {
            throw new ValidationException("El nombre completo no puede exceder 200 caracteres");
        }
    }
}
