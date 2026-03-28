using System.Text.RegularExpressions;
using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un número telefónico válido (formato mexicano).
/// Valida y normaliza números de teléfono de 10 dígitos.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    /// <summary>
    /// Regex para validar teléfonos mexicanos: 10 dígitos, opcionalmente con código de país +52
    /// </summary>
    private static readonly Regex ValidationRegex = new(
        @"^(\+52)?[1-9]\d{9}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Número telefónico normalizado (solo dígitos, sin espacios ni guiones)
    /// </summary>
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Crea un PhoneNumber desde un string, validando y normalizando el formato
    /// </summary>
    /// <param name="value">Número telefónico a validar</param>
    /// <returns>PhoneNumber válido y normalizado</returns>
    /// <exception cref="ValidationException">Si el número no es válido</exception>
    public static PhoneNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("El número telefónico no puede estar vacío");

        // Normalizar: remover espacios, guiones, paréntesis
        var normalized = NormalizePhoneNumber(value);

        // Validar formato mexicano
        if (!ValidationRegex.IsMatch(normalized))
            throw new ValidationException(
                $"El número telefónico '{value}' no es válido. " +
                "Debe contener 10 dígitos (formato mexicano), opcionalmente con código de país +52"
            );

        // Remover código de país si existe para almacenar solo 10 dígitos
        var finalValue = normalized.StartsWith("+52")
            ? normalized.Substring(3)
            : normalized;

        return new PhoneNumber(finalValue);
    }

    /// <summary>
    /// Normaliza el número telefónico removiendo caracteres no numéricos (excepto + inicial)
    /// </summary>
    private static string NormalizePhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Preservar + inicial si existe
        bool hasCountryCode = value.TrimStart().StartsWith("+");

        // Remover todo excepto dígitos y + inicial
        var normalized = Regex.Replace(value, @"[^\d+]", "");

        // Asegurar que + solo esté al inicio
        if (hasCountryCode && normalized.StartsWith("+"))
            return normalized;

        // Si no tiene +, remover cualquier + que pudiera haber quedado
        return normalized.Replace("+", "");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Conversión implícita a string
    /// </summary>
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
