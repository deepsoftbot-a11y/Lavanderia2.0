using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Roles;

public sealed class RolePure : AggregateRoot<RoleId>
{
    private readonly List<int> _permissionIds = new();

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<int> PermissionIds => _permissionIds.AsReadOnly();

    private RolePure() { }

    public static RolePure Create(string name, string? description, bool isActive, IEnumerable<int> permissionIds)
    {
        ValidateName(name);

        var role = new RolePure
        {
            Id = RoleId.Empty(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = isActive
        };

        role._permissionIds.AddRange(permissionIds.Distinct());

        return role;
    }

    internal static RolePure Reconstitute(RoleId id, string name, string? description, bool isActive, IEnumerable<int> permissionIds)
    {
        var role = new RolePure
        {
            Id = id,
            Name = name,
            Description = description,
            IsActive = isActive
        };

        role._permissionIds.AddRange(permissionIds);

        return role;
    }

    public void Update(string name, string? description)
    {
        ValidateName(name);
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void SetPermissions(IEnumerable<int> permissionIds)
    {
        _permissionIds.Clear();
        _permissionIds.AddRange(permissionIds.Distinct());
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new BusinessRuleException("El rol ya está activo");
        }

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new BusinessRuleException("El rol ya está inactivo");
        }

        IsActive = false;
    }

    internal void SetId(RoleId id)
    {
        Id = id;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("El nombre del rol no puede estar vacío");
        }

        if (name.Length > 100)
        {
            throw new ValidationException("El nombre del rol no puede exceder 100 caracteres");
        }
    }
}
