using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa los totales de pagos de un día.
/// Agrupa los montos por método de pago.
/// </summary>
public sealed class DayPaymentTotals : ValueObject
{
    /// <summary>
    /// Total en efectivo
    /// </summary>
    public Money TotalCash { get; }

    /// <summary>
    /// Total en tarjeta
    /// </summary>
    public Money TotalCard { get; }

    /// <summary>
    /// Total en transferencia
    /// </summary>
    public Money TotalTransfer { get; }

    /// <summary>
    /// Total en otros métodos de pago
    /// </summary>
    public Money TotalOther { get; }

    /// <summary>
    /// Total general de todos los pagos
    /// </summary>
    public Money TotalPaid { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private DayPaymentTotals(
        Money totalCash,
        Money totalCard,
        Money totalTransfer,
        Money totalOther,
        Money totalPaid)
    {
        TotalCash = totalCash;
        TotalCard = totalCard;
        TotalTransfer = totalTransfer;
        TotalOther = totalOther;
        TotalPaid = totalPaid;
    }

    /// <summary>
    /// Crea una instancia de DayPaymentTotals
    /// </summary>
    public static DayPaymentTotals Create(
        Money totalCash,
        Money totalCard,
        Money totalTransfer,
        Money totalOther)
    {
        // Calcular total pagado
        var totalPaid = totalCash + totalCard + totalTransfer + totalOther;

        return new DayPaymentTotals(
            totalCash,
            totalCard,
            totalTransfer,
            totalOther,
            totalPaid);
    }

    /// <summary>
    /// Crea una instancia vacía (sin pagos)
    /// </summary>
    public static DayPaymentTotals Empty()
    {
        return new DayPaymentTotals(
            Money.Zero(),
            Money.Zero(),
            Money.Zero(),
            Money.Zero(),
            Money.Zero());
    }

    /// <summary>
    /// Obtiene los componentes para comparación de igualdad
    /// </summary>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TotalCash;
        yield return TotalCard;
        yield return TotalTransfer;
        yield return TotalOther;
        yield return TotalPaid;
    }

    /// <summary>
    /// Representación en string
    /// </summary>
    public override string ToString()
    {
        return $"Cash: {TotalCash}, Card: {TotalCard}, Transfer: {TotalTransfer}, Other: {TotalOther}, Total: {TotalPaid}";
    }
}
