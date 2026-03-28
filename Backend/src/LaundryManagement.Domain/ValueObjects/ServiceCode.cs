using System.Text.RegularExpressions;
using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el código único de un servicio.
/// Formato esperado: Alfanumérico, mayúsculas, hasta 20 caracteres.
/// Ejemplo: "LAVADO", "PLANCHADO", "TINTORERIA"
/// </summary>
public sealed class ServiceCode : ValueObject
{
    private static readonly Regex CodeRegex = new Regex(@"^[A-Z0-9_-]{2,20}$", RegexOptions.Compiled);

    /// <summary>
    /// Valor del código
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private ServiceCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("El código del servicio no puede estar vacío");

        var normalized = value.Trim().ToUpperInvariant();

        if (!CodeRegex.IsMatch(normalized))
            throw new ValidationException(
                "El código del servicio debe tener entre 2 y 20 caracteres alfanuméricos en mayúsculas (se permiten guiones y guiones bajos)");

        Value = normalized;
    }

    /// <summary>
    /// Crea un ServiceCode desde un string
    /// </summary>
    public static ServiceCode From(string value) => new ServiceCode(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Conversión implícita a string
    /// </summary>
    public static implicit operator string(ServiceCode code) => code.Value;
}
