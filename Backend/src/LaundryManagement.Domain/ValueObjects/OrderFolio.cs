using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el folio único de una orden.
/// Formato: ORD-YYYYMMDD-NNNN (ej: ORD-20260104-0001)
/// </summary>
public sealed class OrderFolio : ValueObject
{
    private const string FolioPattern = @"^ORD-\d{8}-\d{4}$";
    private static readonly Regex FolioRegex = new(FolioPattern, RegexOptions.Compiled);

    /// <summary>
    /// Valor del folio
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructor privado para crear una instancia de OrderFolio
    /// </summary>
    private OrderFolio(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("El folio de la orden no puede estar vacío");

        if (!FolioRegex.IsMatch(value))
            throw new ValidationException(
                $"El folio de la orden debe tener el formato ORD-YYYYMMDD-NNNN. Valor recibido: {value}");

        Value = value;
    }

    /// <summary>
    /// Crea una instancia de OrderFolio desde un string
    /// </summary>
    public static OrderFolio FromString(string value)
    {
        return new OrderFolio(value);
    }

    /// <summary>
    /// Genera un nuevo folio basado en la fecha y un número secuencial
    /// </summary>
    /// <param name="date">Fecha para el folio</param>
    /// <param name="sequenceNumber">Número secuencial del día</param>
    public static OrderFolio Generate(DateTime date, int sequenceNumber)
    {
        if (sequenceNumber < 1 || sequenceNumber > 9999)
            throw new ValidationException("El número secuencial debe estar entre 1 y 9999");

        string datePart = date.ToString("yyyyMMdd");
        string numberPart = sequenceNumber.ToString("D4");
        string folioValue = $"ORD-{datePart}-{numberPart}";

        return new OrderFolio(folioValue);
    }

    /// <summary>
    /// Extrae la fecha del folio
    /// </summary>
    public DateTime ExtractDate()
    {
        // Formato: ORD-YYYYMMDD-NNNN
        string datePart = Value.Substring(4, 8); // Extrae YYYYMMDD
        return DateTime.ParseExact(datePart, "yyyyMMdd", null);
    }

    /// <summary>
    /// Extrae el número secuencial del folio
    /// </summary>
    public int ExtractSequenceNumber()
    {
        // Formato: ORD-YYYYMMDD-NNNN
        string numberPart = Value.Substring(13, 4); // Extrae NNNN
        return int.Parse(numberPart);
    }

    /// <summary>
    /// Obtiene los componentes para comparación de igualdad
    /// </summary>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Representación en string del folio
    /// </summary>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Conversión implícita a string
    /// </summary>
    public static implicit operator string(OrderFolio folio)
    {
        return folio.Value;
    }
}
