using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

public sealed class RoleId : ValueObject
{
    public int Value { get; }

    private RoleId(int value)
    {
        Value = value;
    }

    public static RoleId From(int value)
    {
        if (value <= 0)
        {
            throw new ValidationException("RoleId debe ser mayor a 0");
        }

        return new RoleId(value);
    }

    public static RoleId Empty() => new(0);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(RoleId roleId) => roleId.Value;
}
