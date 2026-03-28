using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el tipo de unidad de cobro de un servicio.
/// PIECE = Cobro por pieza individual (cada prenda tiene precio)
/// WEIGHT = Cobro por peso (kg, con rango de peso mínimo/máximo)
/// </summary>
public sealed class UnitType : ValueObject
{
    public enum Type
    {
        PIECE,
        WEIGHT
    }

    /// <summary>
    /// Tipo de unidad
    /// </summary>
    public Type Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private UnitType(Type value)
    {
        Value = value;
    }

    /// <summary>
    /// Crea un UnitType desde enum
    /// </summary>
    public static UnitType From(Type value) => new UnitType(value);

    /// <summary>
    /// Crea un UnitType desde string (DB TipoCobroServicio)
    /// Acepta: "PIEZA", "PorPieza", "KILO", "PorPeso", "PorKilo"
    /// </summary>
    public static UnitType FromDatabaseValue(string dbValue)
    {
        if (string.IsNullOrWhiteSpace(dbValue))
            throw new ValidationException("El tipo de cobro no puede estar vacío");

        var normalized = dbValue.Trim().ToUpperInvariant();

        return normalized switch
        {
            "PIEZA" or "PORPIEZA" => new UnitType(Type.PIECE),
            "KILO" or "PORPESO" or "PORKILO" or "PESO" => new UnitType(Type.WEIGHT),
            _ => throw new ValidationException($"Tipo de cobro no válido: {dbValue}. Se esperaba 'PIEZA', 'PorPieza', 'KILO', 'PorPeso' o 'PorKilo'")
        };
    }

    /// <summary>
    /// Convierte a valor de base de datos
    /// Retorna en el formato usado por la BD: "PorPieza" o "PorPeso"
    /// </summary>
    public string ToDatabaseValue()
    {
        return Value switch
        {
            Type.PIECE => "PorPieza",
            Type.WEIGHT => "PorPeso",
            _ => throw new InvalidOperationException($"Tipo de unidad no soportado: {Value}")
        };
    }

    /// <summary>
    /// Crea UnitType para cobro por pieza
    /// </summary>
    public static UnitType Piece() => new UnitType(Type.PIECE);

    /// <summary>
    /// Crea UnitType para cobro por peso
    /// </summary>
    public static UnitType Weight() => new UnitType(Type.WEIGHT);

    public bool IsPiece => Value == Type.PIECE;
    public bool IsWeight => Value == Type.WEIGHT;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
