using System.Text.RegularExpressions;
using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un RFC (Registro Federal de Contribuyentes) mexicano válido.
/// Valida el formato de 12-13 caracteres para RFC de personas físicas y morales.
/// </summary>
public sealed class RFC : ValueObject
{
    /// <summary>
    /// Regex para validar RFC mexicano:
    /// - Persona Moral: 3 letras + 6 dígitos + 3 alfanuméricos = 12 caracteres
    /// - Persona Física: 4 letras + 6 dígitos + 3 alfanuméricos = 13 caracteres
    /// Formato: [A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}
    /// </summary>
    private static readonly Regex ValidationRegex = new(
        @"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// RFC en formato normalizado (mayúsculas, sin espacios)
    /// </summary>
    public string Value { get; }

    private RFC(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Crea un RFC desde un string, validando el formato mexicano
    /// </summary>
    /// <param name="value">RFC a validar</param>
    /// <returns>RFC válido y normalizado</returns>
    /// <exception cref="ValidationException">Si el RFC no es válido</exception>
    public static RFC From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("El RFC no puede estar vacío");

        // Normalizar: mayúsculas y sin espacios
        var normalized = NormalizeRFC(value);

        // Validar longitud
        if (normalized.Length < 12 || normalized.Length > 13)
            throw new ValidationException(
                $"El RFC '{value}' debe tener 12 o 13 caracteres. " +
                "12 para persona moral, 13 para persona física"
            );

        // Validar formato
        if (!ValidationRegex.IsMatch(normalized))
            throw new ValidationException(
                $"El RFC '{value}' no tiene un formato válido. " +
                "Formato esperado: 3-4 letras + 6 dígitos + 3 caracteres alfanuméricos (ejemplo: ABC123456XYZ)"
            );

        return new RFC(normalized);
    }

    /// <summary>
    /// Crea un RFC opcional que puede ser null si el valor está vacío
    /// </summary>
    /// <param name="value">RFC opcional</param>
    /// <returns>RFC válido o null si el valor está vacío</returns>
    public static RFC? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return From(value);
    }

    /// <summary>
    /// Normaliza el RFC a mayúsculas y sin espacios
    /// </summary>
    private static string NormalizeRFC(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Convertir a mayúsculas y remover espacios
        return value.ToUpperInvariant().Replace(" ", "").Replace("-", "");
    }

    /// <summary>
    /// Indica si el RFC es de persona física (13 caracteres)
    /// </summary>
    public bool IsPersonaFisica => Value.Length == 13;

    /// <summary>
    /// Indica si el RFC es de persona moral (12 caracteres)
    /// </summary>
    public bool IsPersonaMoral => Value.Length == 12;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Conversión implícita a string
    /// </summary>
    public static implicit operator string(RFC rfc) => rfc.Value;
}
