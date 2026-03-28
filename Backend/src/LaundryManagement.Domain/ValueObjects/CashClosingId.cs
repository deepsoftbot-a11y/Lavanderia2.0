using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de un corte de caja.
/// Encapsula el ID como concepto de dominio, no solo un int primitivo.
/// </summary>
public sealed class CashClosingId : ValueObject
{
    /// <summary>
    /// Valor del identificador
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private CashClosingId(int value)
    {
        if (value < 0)
            throw new ArgumentException("El CashClosingId no puede ser negativo", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Crea un CashClosingId desde un valor int
    /// </summary>
    public static CashClosingId From(int value) => new CashClosingId(value);

    /// <summary>
    /// Crea un CashClosingId temporal (para nuevos cortes antes de persistir)
    /// </summary>
    public static CashClosingId Empty() => new CashClosingId(0);

    /// <summary>
    /// Verifica si el ID está vacío (corte no persistido aún)
    /// </summary>
    public bool IsEmpty => Value == 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Conversión implícita a int
    /// </summary>
    public static implicit operator int(CashClosingId cashClosingId) => cashClosingId.Value;
}
