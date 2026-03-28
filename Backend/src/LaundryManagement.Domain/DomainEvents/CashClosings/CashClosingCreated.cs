using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.CashClosings;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo corte de caja
/// </summary>
public sealed class CashClosingCreated : DomainEvent
{
    /// <summary>
    /// Identificador del corte de caja creado
    /// </summary>
    public int CashClosingId { get; }

    /// <summary>
    /// Identificador del cajero
    /// </summary>
    public int CashierId { get; }

    /// <summary>
    /// Fecha del corte
    /// </summary>
    public DateTime ClosingDate { get; }

    /// <summary>
    /// Total esperado
    /// </summary>
    public decimal TotalExpected { get; }

    /// <summary>
    /// Total declarado
    /// </summary>
    public decimal TotalDeclared { get; }

    /// <summary>
    /// Diferencia final
    /// </summary>
    public decimal FinalDifference { get; }

    /// <summary>
    /// Constructor del evento CashClosingCreated
    /// </summary>
    public CashClosingCreated(
        int cashClosingId,
        int cashierId,
        DateTime closingDate,
        decimal totalExpected,
        decimal totalDeclared,
        decimal finalDifference)
    {
        CashClosingId = cashClosingId;
        CashierId = cashierId;
        ClosingDate = closingDate;
        TotalExpected = totalExpected;
        TotalDeclared = totalDeclared;
        FinalDifference = finalDifference;
    }
}
