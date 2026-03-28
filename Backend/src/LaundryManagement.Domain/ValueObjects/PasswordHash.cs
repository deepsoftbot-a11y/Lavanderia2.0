using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object representing a hashed password (never stores plain text)
/// </summary>
public sealed class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PasswordHash from an already hashed password (for reconstitution from database)
    /// </summary>
    public static PasswordHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ValidationException("El hash de contraseña no puede estar vacío");
        }

        if (hash.Length > 255)
        {
            throw new ValidationException("El hash de contraseña no puede exceder 255 caracteres");
        }

        return new PasswordHash(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns protected string instead of actual hash for security
    /// </summary>
    public override string ToString() => "[PROTECTED]";

    public static implicit operator string(PasswordHash passwordHash) => passwordHash.Value;
}
