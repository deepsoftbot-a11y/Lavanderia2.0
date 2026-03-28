using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object representing a User identifier
/// </summary>
public sealed class UserId : ValueObject
{
    public int Value { get; }

    private UserId(int value)
    {
        Value = value;
    }

    public static UserId From(int value)
    {
        if (value <= 0)
        {
            throw new ValidationException("UserId debe ser mayor a 0");
        }

        return new UserId(value);
    }

    public static UserId Empty() => new(0);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(UserId userId) => userId.Value;
}
