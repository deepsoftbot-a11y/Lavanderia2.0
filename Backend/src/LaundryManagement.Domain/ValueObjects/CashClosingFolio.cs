using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el folio único de un corte de caja.
/// Formato secuencial: CORTE-yyyyMMdd-NNNN (ej: CORTE-20260212-0001)
/// También soporta formato legacy timestamp: CORTE-yyyyMMdd-HHmmss
/// </summary>
public sealed class CashClosingFolio : ValueObject
{
    private const string FolioPattern = @"^CORTE-\d{8}-\d{4,6}$";
    private static readonly Regex FolioRegex = new(FolioPattern, RegexOptions.Compiled);

    /// <summary>
    /// Valor del folio
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructor privado para crear una instancia de CashClosingFolio
    /// </summary>
    private CashClosingFolio(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("El folio del corte de caja no puede estar vacío");

        if (!FolioRegex.IsMatch(value))
            throw new ValidationException(
                $"El folio del corte de caja debe tener el formato CORTE-YYYYMMDD-NNNN. Valor recibido: {value}");

        Value = value;
    }

    /// <summary>
    /// Crea una instancia de CashClosingFolio desde un string con validación
    /// </summary>
    public static CashClosingFolio From(string value)
    {
        return new CashClosingFolio(value);
    }

    /// <summary>
    /// Crea una instancia de CashClosingFolio desde un string (para reconstitución desde BD)
    /// </summary>
    public static CashClosingFolio FromString(string value)
    {
        return new CashClosingFolio(value);
    }

    /// <summary>
    /// Genera un nuevo folio secuencial basado en la fecha y un número de secuencia.
    /// Formato: CORTE-yyyyMMdd-NNNN (ej: CORTE-20260212-0001)
    /// </summary>
    public static CashClosingFolio FromSequential(DateTime date, int sequenceNumber)
    {
        if (sequenceNumber <= 0)
            throw new ValidationException("El número secuencial debe ser mayor a 0");

        string folio = $"CORTE-{date:yyyyMMdd}-{sequenceNumber:D4}";
        return new CashClosingFolio(folio);
    }

    /// <summary>
    /// Extrae la fecha del folio.
    /// Formato: CORTE-YYYYMMDD-NNNN o CORTE-YYYYMMDD-HHMMSS
    /// </summary>
    public DateTime ExtractDate()
    {
        // "CORTE-" tiene 6 caracteres, luego siguen 8 caracteres de fecha
        string datePart = Value.Substring(6, 8);
        return DateTime.ParseExact(datePart, "yyyyMMdd", null);
    }

    /// <summary>
    /// Extrae el número secuencial del folio (dígitos después del segundo guión).
    /// </summary>
    public int ExtractSequenceNumber()
    {
        string numberPart = Value.Substring(Value.LastIndexOf('-') + 1);
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
    public static implicit operator string(CashClosingFolio folio)
    {
        return folio.Value;
    }
}
