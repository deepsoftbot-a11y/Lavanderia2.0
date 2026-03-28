using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.CashClosings;

/// <summary>
/// Entidad hija del agregado CashClosingPure.
/// Representa el detalle de un corte de caja por método de pago.
/// </summary>
public sealed class CashClosingDetail : Entity<int>
{
    /// <summary>
    /// Identificador del método de pago (1=Efectivo, 2=Tarjeta, 3=Transferencia, 4=Otros)
    /// </summary>
    public int PaymentMethodId { get; private set; }

    /// <summary>
    /// Número de transacciones para este método de pago
    /// </summary>
    public int TransactionCount { get; private set; }

    /// <summary>
    /// Total esperado para este método de pago
    /// </summary>
    public Money ExpectedTotal { get; private set; }

    /// <summary>
    /// Total declarado por el cajero para este método de pago
    /// </summary>
    public Money DeclaredTotal { get; private set; }

    /// <summary>
    /// Diferencia entre declarado y esperado (puede ser negativa)
    /// </summary>
    public MoneyDifference Difference { get; private set; }

    private CashClosingDetail()
    {
        ExpectedTotal = Money.Zero();
        DeclaredTotal = Money.Zero();
        Difference = MoneyDifference.Zero();
    }

    /// <summary>
    /// Crea un nuevo detalle de corte de caja
    /// </summary>
    internal static CashClosingDetail Create(
        int paymentMethodId,
        Money expected,
        Money declared,
        int transactionCount = 0)
    {
        if (paymentMethodId <= 0)
            throw new ValidationException("El método de pago debe ser válido");

        return new CashClosingDetail
        {
            PaymentMethodId = paymentMethodId,
            TransactionCount = transactionCount,
            ExpectedTotal = expected,
            DeclaredTotal = declared,
            Difference = MoneyDifference.Calculate(declared, expected)
        };
    }

    /// <summary>
    /// Reconstituye un detalle desde la base de datos (usado por Repository/Mapper)
    /// </summary>
    internal static CashClosingDetail Reconstitute(
        int id,
        int paymentMethodId,
        int transactionCount,
        Money expected,
        Money declared,
        MoneyDifference difference)
    {
        return new CashClosingDetail
        {
            Id = id,
            PaymentMethodId = paymentMethodId,
            TransactionCount = transactionCount,
            ExpectedTotal = expected,
            DeclaredTotal = declared,
            Difference = difference
        };
    }
}
