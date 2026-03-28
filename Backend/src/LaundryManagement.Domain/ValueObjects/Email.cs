using System.Text.RegularExpressions;
using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object representing an Email address with validation
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex ValidationRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("El email no puede estar vacío");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (!ValidationRegex.IsMatch(normalized))
        {
            throw new ValidationException("El formato del email no es válido");
        }

        if (normalized.Length > 255)
        {
            throw new ValidationException("El email no puede exceder 255 caracteres");
        }

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
