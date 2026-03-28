using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object representing a user role with predefined valid values
/// </summary>
public sealed class UserRole : ValueObject
{
    public const string AdminRoleName = "admin";
    public const string EmpleadoRoleName = "empleado";

    public string Value { get; }

    private UserRole(string value)
    {
        Value = value;
    }

    public static UserRole Admin => new(AdminRoleName);
    public static UserRole Empleado => new(EmpleadoRoleName);

    public static UserRole From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("El rol no puede estar vacío");
        }

        return new UserRole(value.Trim().ToLowerInvariant());
    }

    public bool IsAdmin => Value == AdminRoleName;
    public bool IsEmpleado => Value == EmpleadoRoleName;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(UserRole role) => role.Value;
}
