using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un monto monetario.
/// Encapsula la cantidad y la moneda, asegurando invariantes como no negatividad.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Cantidad del monto monetario
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Código de la moneda (por defecto MXN - Peso Mexicano)
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Constructor privado para crear una instancia de Money
    /// </summary>
    private Money(decimal amount, string currency = "MXN")
    {
        if (amount < 0)
            throw new ValidationException("El monto no puede ser negativo");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Crea una instancia de Money desde un valor decimal
    /// </summary>
    public static Money FromDecimal(decimal amount, string currency = "MXN")
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Crea una instancia de Money con valor cero
    /// </summary>
    public static Money Zero(string currency = "MXN")
    {
        return new Money(0, currency);
    }

    /// <summary>
    /// Suma dos montos monetarios
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Resta dos montos monetarios
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplica el monto por un factor
    /// </summary>
    public Money Multiply(decimal multiplier)
    {
        return new Money(Amount * multiplier, Currency);
    }

    /// <summary>
    /// Aplica un porcentaje al monto
    /// </summary>
    /// <param name="percentage">Porcentaje a aplicar (ej: 10 para 10%)</param>
    public Money ApplyPercentage(decimal percentage)
    {
        return new Money(Amount * (percentage / 100), Currency);
    }

    /// <summary>
    /// Verifica si el monto es mayor que otro
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    /// <summary>
    /// Verifica si el monto es mayor o igual que otro
    /// </summary>
    public bool IsGreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);
        return Amount >= other.Amount;
    }

    /// <summary>
    /// Verifica si el monto es menor que otro
    /// </summary>
    public bool IsLessThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount < other.Amount;
    }

    /// <summary>
    /// Verifica si el monto es cero
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Asegura que ambos montos tengan la misma moneda
    /// </summary>
    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"No se pueden operar montos con diferentes monedas: {Currency} vs {other.Currency}");
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
    /// Representación en string del monto
    /// </summary>
    public override string ToString()
    {
        return $"{Amount:N2} {Currency}";
    }

    /// <summary>
    /// Operador de suma
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        return left.Add(right);
    }

    /// <summary>
    /// Operador de resta
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        return left.Subtract(right);
    }

    /// <summary>
    /// Operador de multiplicación
    /// </summary>
    public static Money operator *(Money money, decimal multiplier)
    {
        return money.Multiply(multiplier);
    }

    /// <summary>
    /// Operador de mayor que
    /// </summary>
    public static bool operator >(Money left, Money right)
    {
        return left.IsGreaterThan(right);
    }

    /// <summary>
    /// Operador de menor que
    /// </summary>
    public static bool operator <(Money left, Money right)
    {
        return left.IsLessThan(right);
    }

    /// <summary>
    /// Operador de mayor o igual que
    /// </summary>
    public static bool operator >=(Money left, Money right)
    {
        return left.IsGreaterThanOrEqual(right);
    }

    /// <summary>
    /// Operador de menor o igual que
    /// </summary>
    public static bool operator <=(Money left, Money right)
    {
        return !left.IsGreaterThan(right);
    }
}
