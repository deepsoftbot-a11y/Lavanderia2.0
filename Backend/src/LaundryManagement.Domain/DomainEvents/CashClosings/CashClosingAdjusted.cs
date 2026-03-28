using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.CashClosings;

/// <summary>
/// Evento de dominio que se dispara cuando se ajusta un corte de caja
/// </summary>
public sealed class CashClosingAdjusted : DomainEvent
{
    /// <summary>
    /// Identificador del corte de caja ajustado
    /// </summary>
    public int CashClosingId { get; }

    /// <summary>
    /// Monto del ajuste
    /// </summary>
    public decimal AdjustmentAmount { get; }

    /// <summary>
    /// Motivo del ajuste
    /// </summary>
    public string AdjustmentReason { get; }

    /// <summary>
    /// Usuario que realizó el ajuste
    /// </summary>
    public int AdjustedBy { get; }

    /// <summary>
    /// Fecha del ajuste
    /// </summary>
    public DateTime AdjustmentDate { get; }

    /// <summary>
    /// Constructor del evento CashClosingAdjusted
    /// </summary>
    public CashClosingAdjusted(
        int cashClosingId,
        decimal adjustmentAmount,
        string adjustmentReason,
        int adjustedBy,
        DateTime adjustmentDate)
    {
        CashClosingId = cashClosingId;
        AdjustmentAmount = adjustmentAmount;
        AdjustmentReason = adjustmentReason;
        AdjustedBy = adjustedBy;
        AdjustmentDate = adjustmentDate;
    }
}
