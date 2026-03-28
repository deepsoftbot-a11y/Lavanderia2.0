using System.Text.RegularExpressions;
using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object representing a Username with validation
/// </summary>
public sealed class Username : ValueObject
{
    private static readonly Regex ValidationRegex = new(
        @"^[a-zA-Z0-9._-]{3,50}$",
        RegexOptions.Compiled
    );

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("El nombre de usuario no puede estar vacío");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (!ValidationRegex.IsMatch(normalized))
        {
            throw new ValidationException(
                "El nombre de usuario debe tener entre 3 y 50 caracteres y solo puede contener letras, números, puntos, guiones y guiones bajos"
            );
        }

        return new Username(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Username username) => username.Value;
}
