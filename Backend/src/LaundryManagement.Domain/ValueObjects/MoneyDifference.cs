using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una diferencia monetaria.
/// A diferencia de Money, permite valores negativos para representar faltantes.
/// - Positivo: Sobrante (declarado > esperado)
/// - Negativo: Faltante (declarado < esperado)
/// </summary>
public sealed class MoneyDifference : ValueObject
{
    /// <summary>
    /// Cantidad de la diferencia (puede ser negativa)
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Código de la moneda (por defecto MXN - Peso Mexicano)
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Indica si hay faltante (diferencia negativa)
    /// </summary>
    public bool IsDeficit => Amount < 0;

    /// <summary>
    /// Indica si hay sobrante (diferencia positiva)
    /// </summary>
    public bool IsSurplus => Amount > 0;

    /// <summary>
    /// Indica si la diferencia es cero
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Valor absoluto de la diferencia
    /// </summary>
    public decimal AbsoluteValue => Math.Abs(Amount);

    /// <summary>
    /// Constructor privado
    /// </summary>
    private MoneyDifference(decimal amount, string currency = "MXN")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Crea una instancia de MoneyDifference desde un valor decimal
    /// </summary>
    public static MoneyDifference FromDecimal(decimal amount, string currency = "MXN")
    {
        return new MoneyDifference(amount, currency);
    }

    /// <summary>
    /// Calcula la diferencia entre dos montos (declarado - esperado)
    /// </summary>
    public static MoneyDifference Calculate(Money declared, Money expected)
    {
        if (declared.Currency != expected.Currency)
            throw new InvalidOperationException(
                $"No se pueden calcular diferencias con diferentes monedas: {declared.Currency} vs {expected.Currency}");

        return new MoneyDifference(declared.Amount - expected.Amount, declared.Currency);
    }

    /// <summary>
    /// Crea una instancia con valor cero
    /// </summary>
    public static MoneyDifference Zero(string currency = "MXN")
    {
        return new MoneyDifference(0, currency);
    }

    /// <summary>
    /// Suma dos diferencias monetarias
    /// </summary>
    public MoneyDifference Add(MoneyDifference other)
    {
        EnsureSameCurrency(other.Currency);
        return new MoneyDifference(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Suma un ajuste (Money) a la diferencia
    /// </summary>
    public MoneyDifference AddMoney(Money adjustment)
    {
        if (Currency != adjustment.Currency)
            throw new InvalidOperationException(
                $"No se pueden operar montos con diferentes monedas: {Currency} vs {adjustment.Currency}");

        return new MoneyDifference(Amount + adjustment.Amount, Currency);
    }

    /// <summary>
    /// Asegura que ambos montos tengan la misma moneda
    /// </summary>
    private void EnsureSameCurrency(string otherCurrency)
    {
        if (Currency != otherCurrency)
            throw new InvalidOperationException(
                $"No se pueden operar diferencias con diferentes monedas: {Currency} vs {otherCurrency}");
    }

    /// <summary>
    /// Obtiene los componentes para comparación de igualdad
    /// </summary>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Representación en string de la diferencia
    /// </summary>
    public override string ToString()
    {
        var sign = IsDeficit ? "-" : (IsSurplus ? "+" : "");
        return $"{sign}{AbsoluteValue:N2} {Currency}";
    }

    /// <summary>
    /// Operador de suma entre diferencias
    /// </summary>
    public static MoneyDifference operator +(MoneyDifference left, MoneyDifference right)
    {
        return left.Add(right);
    }

    /// <summary>
    /// Operador de suma entre diferencia y Money (ajuste)
    /// </summary>
    public static MoneyDifference operator +(MoneyDifference diff, Money money)
    {
        return diff.AddMoney(money);
    }
}
