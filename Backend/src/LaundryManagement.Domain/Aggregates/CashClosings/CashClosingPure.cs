using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.DomainEvents.CashClosings;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.CashClosings;

/// <summary>
/// Agregado CashClosing PURO - Entidad de dominio rica completamente independiente de infraestructura.
/// NO conoce EF Core, NO conoce base de datos, solo lógica de negocio.
/// </summary>
public sealed class CashClosingPure : AggregateRoot<CashClosingId>
{
    #region Propiedades de Dominio

    /// <summary>
    /// Folio único del corte de caja (null hasta que el repositorio lo asigne)
    /// </summary>
    public CashClosingFolio? Folio { get; private set; }

    /// <summary>
    /// Identificador del cajero
    /// </summary>
    public int CashierId { get; private set; }

    /// <summary>
    /// Descripción del turno
    /// </summary>
    public string? ShiftDescription { get; private set; }

    /// <summary>
    /// Fecha de inicio del período
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Fecha de fin del período
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Fecha en que se realizó el corte
    /// </summary>
    public DateTime ClosingDate { get; private set; }

    /// <summary>
    /// Fondo inicial de caja
    /// </summary>
    public Money? InitialFund { get; private set; }

    #region Detalles por método de pago

    private readonly List<CashClosingDetail> _details = new();

    /// <summary>
    /// Detalles del corte por método de pago
    /// </summary>
    public IReadOnlyCollection<CashClosingDetail> Details => _details.AsReadOnly();

    #endregion

    #region Totales Esperados (calculados por el sistema)

    /// <summary>
    /// Total esperado en efectivo
    /// </summary>
    public Money ExpectedCash { get; private set; }

    /// <summary>
    /// Total esperado en tarjeta
    /// </summary>
    public Money ExpectedCard { get; private set; }

    /// <summary>
    /// Total esperado en transferencia
    /// </summary>
    public Money ExpectedTransfer { get; private set; }

    /// <summary>
    /// Total esperado en otros métodos de pago
    /// </summary>
    public Money ExpectedOther { get; private set; }

    /// <summary>
    /// Total general esperado
    /// </summary>
    public Money TotalExpected { get; private set; }

    #endregion

    #region Totales Declarados (reportados por el cajero)

    /// <summary>
    /// Total declarado en efectivo
    /// </summary>
    public Money DeclaredCash { get; private set; }

    /// <summary>
    /// Total declarado en tarjeta
    /// </summary>
    public Money DeclaredCard { get; private set; }

    /// <summary>
    /// Total declarado en transferencia
    /// </summary>
    public Money DeclaredTransfer { get; private set; }

    /// <summary>
    /// Total declarado en otros métodos de pago
    /// </summary>
    public Money DeclaredOther { get; private set; }

    /// <summary>
    /// Total general declarado
    /// </summary>
    public Money TotalDeclared { get; private set; }

    #endregion

    #region Diferencias

    /// <summary>
    /// Diferencia inicial en efectivo (Declarado - Esperado)
    /// </summary>
    public MoneyDifference InitialDifferenceCash { get; private set; }

    /// <summary>
    /// Diferencia inicial en tarjeta (Declarado - Esperado)
    /// </summary>
    public MoneyDifference InitialDifferenceCard { get; private set; }

    /// <summary>
    /// Diferencia inicial en transferencia (Declarado - Esperado)
    /// </summary>
    public MoneyDifference InitialDifferenceTransfer { get; private set; }

    /// <summary>
    /// Diferencia inicial en otros (Declarado - Esperado)
    /// </summary>
    public MoneyDifference InitialDifferenceOther { get; private set; }

    /// <summary>
    /// Diferencia inicial total
    /// </summary>
    public MoneyDifference InitialDifference { get; private set; }

    /// <summary>
    /// Diferencia final después del ajuste
    /// </summary>
    public MoneyDifference FinalDifference { get; private set; }

    #endregion

    #region Ajuste

    /// <summary>
    /// Monto del ajuste
    /// </summary>
    public Money AdjustmentAmount { get; private set; }

    /// <summary>
    /// Motivo del ajuste
    /// </summary>
    public string? AdjustmentReason { get; private set; }

    /// <summary>
    /// Fecha del ajuste
    /// </summary>
    public DateTime? AdjustmentDate { get; private set; }

    #endregion

    /// <summary>
    /// Número de transacciones en el período
    /// </summary>
    public int TransactionCount { get; private set; }

    /// <summary>
    /// Observaciones generales
    /// </summary>
    public string? Notes { get; private set; }

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado para reconstitución desde BD
    /// </summary>
    private CashClosingPure()
    {
        // Folio = null (el repositorio lo asignará antes de persistir)
        ExpectedCash = ExpectedCard = ExpectedTransfer = ExpectedOther = TotalExpected = Money.Zero();
        DeclaredCash = DeclaredCard = DeclaredTransfer = DeclaredOther = TotalDeclared = Money.Zero();
        InitialDifferenceCash = InitialDifferenceCard = InitialDifferenceTransfer = InitialDifferenceOther = MoneyDifference.Zero();
        InitialDifference = FinalDifference = MoneyDifference.Zero();
        AdjustmentAmount = Money.Zero();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea un nuevo corte de caja
    /// </summary>
    public static CashClosingPure Create(
        int cashierId,
        DateTime startDate,
        DateTime endDate,
        Money expectedCash,
        Money expectedCard,
        Money expectedTransfer,
        Money expectedOther,
        Money declaredCash,
        Money declaredCard,
        Money declaredTransfer,
        Money declaredOther,
        Money adjustmentAmount,
        string? adjustmentReason = null,
        string? shiftDescription = null,
        string? notes = null,
        Money? initialFund = null)
    {
        // Validaciones
        if (cashierId <= 0)
            throw new ValidationException("El CashierId debe ser un usuario válido");

        if (endDate < startDate)
            throw new BusinessRuleException("La fecha de fin no puede ser anterior a la fecha de inicio");

        var closing = new CashClosingPure
        {
            Id = CashClosingId.Empty(),
            Folio = null, // El repositorio lo asignará antes de persistir
            CashierId = cashierId,
            ShiftDescription = shiftDescription,
            StartDate = startDate,
            EndDate = endDate,
            ClosingDate = DateTime.Now,
            InitialFund = initialFund,
            ExpectedCash = expectedCash,
            ExpectedCard = expectedCard,
            ExpectedTransfer = expectedTransfer,
            ExpectedOther = expectedOther,
            DeclaredCash = declaredCash,
            DeclaredCard = declaredCard,
            DeclaredTransfer = declaredTransfer,
            DeclaredOther = declaredOther,
            AdjustmentAmount = adjustmentAmount,
            AdjustmentReason = adjustmentReason,
            AdjustmentDate = adjustmentAmount.IsZero ? null : DateTime.Now,
            Notes = notes,
            TransactionCount = 0
        };

        // Calcular totales y diferencias
        closing.CalculateTotalsAndDifferences();

        // Crear detalles por método de pago
        closing._details.Add(CashClosingDetail.Create(1, expectedCash, declaredCash));         // Efectivo
        closing._details.Add(CashClosingDetail.Create(2, expectedCard, declaredCard));         // Tarjeta
        closing._details.Add(CashClosingDetail.Create(3, expectedTransfer, declaredTransfer)); // Transferencia
        closing._details.Add(CashClosingDetail.Create(4, expectedOther, declaredOther));       // Otros

        // Evento de dominio
        closing.RaiseDomainEvent(new CashClosingCreated(
            0, // Se actualizará al persistir
            cashierId,
            closing.ClosingDate,
            closing.TotalExpected.Amount,
            closing.TotalDeclared.Amount,
            closing.FinalDifference.Amount
        ));

        return closing;
    }

    /// <summary>
    /// Reconstituye un corte de caja desde la base de datos (usado por Repository)
    /// </summary>
    internal static CashClosingPure Reconstitute(
        CashClosingId id,
        CashClosingFolio? folio,
        int cashierId,
        string? shiftDescription,
        DateTime startDate,
        DateTime endDate,
        DateTime closingDate,
        Money expectedCash,
        Money expectedCard,
        Money expectedTransfer,
        Money expectedOther,
        Money totalExpected,
        Money declaredCash,
        Money declaredCard,
        Money declaredTransfer,
        Money declaredOther,
        Money totalDeclared,
        MoneyDifference initialDifferenceCash,
        MoneyDifference initialDifferenceCard,
        MoneyDifference initialDifferenceTransfer,
        MoneyDifference initialDifferenceOther,
        MoneyDifference initialDifference,
        Money adjustmentAmount,
        string? adjustmentReason,
        DateTime? adjustmentDate,
        MoneyDifference finalDifference,
        int transactionCount,
        string? notes,
        Money? initialFund = null,
        List<CashClosingDetail>? details = null)
    {
        var closing = new CashClosingPure
        {
            Id = id,
            Folio = folio,
            CashierId = cashierId,
            ShiftDescription = shiftDescription,
            StartDate = startDate,
            EndDate = endDate,
            ClosingDate = closingDate,
            InitialFund = initialFund,
            ExpectedCash = expectedCash,
            ExpectedCard = expectedCard,
            ExpectedTransfer = expectedTransfer,
            ExpectedOther = expectedOther,
            TotalExpected = totalExpected,
            DeclaredCash = declaredCash,
            DeclaredCard = declaredCard,
            DeclaredTransfer = declaredTransfer,
            DeclaredOther = declaredOther,
            TotalDeclared = totalDeclared,
            InitialDifferenceCash = initialDifferenceCash,
            InitialDifferenceCard = initialDifferenceCard,
            InitialDifferenceTransfer = initialDifferenceTransfer,
            InitialDifferenceOther = initialDifferenceOther,
            InitialDifference = initialDifference,
            AdjustmentAmount = adjustmentAmount,
            AdjustmentReason = adjustmentReason,
            AdjustmentDate = adjustmentDate,
            FinalDifference = finalDifference,
            TransactionCount = transactionCount,
            Notes = notes
        };

        if (details != null)
        {
            closing._details.AddRange(details);
        }

        return closing;
    }

    #endregion

    #region Métodos de Dominio

    /// <summary>
    /// Calcula los totales y diferencias del corte
    /// </summary>
    private void CalculateTotalsAndDifferences()
    {
        // Calcular totales
        TotalExpected = ExpectedCash + ExpectedCard + ExpectedTransfer + ExpectedOther;
        TotalDeclared = DeclaredCash + DeclaredCard + DeclaredTransfer + DeclaredOther;

        // Calcular diferencias por método de pago
        InitialDifferenceCash = CalculateDifference(DeclaredCash, ExpectedCash);
        InitialDifferenceCard = CalculateDifference(DeclaredCard, ExpectedCard);
        InitialDifferenceTransfer = CalculateDifference(DeclaredTransfer, ExpectedTransfer);
        InitialDifferenceOther = CalculateDifference(DeclaredOther, ExpectedOther);

        // Calcular diferencia total inicial
        InitialDifference = CalculateDifference(TotalDeclared, TotalExpected);

        // Calcular diferencia final (incluyendo ajuste)
        FinalDifference = CalculateDifferenceWithAdjustment(InitialDifference, AdjustmentAmount);
    }

    /// <summary>
    /// Calcula la diferencia entre dos montos (puede ser negativa)
    /// </summary>
    private MoneyDifference CalculateDifference(Money declared, Money expected)
    {
        return MoneyDifference.Calculate(declared, expected);
    }

    /// <summary>
    /// Calcula la diferencia final incluyendo el ajuste
    /// </summary>
    private MoneyDifference CalculateDifferenceWithAdjustment(MoneyDifference initialDiff, Money adjustment)
    {
        return initialDiff + adjustment;
    }

    /// <summary>
    /// Aplica un ajuste al corte de caja
    /// </summary>
    public void ApplyAdjustment(Money amount, string reason, int adjustedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException("El motivo del ajuste es requerido");

        if (adjustedBy <= 0)
            throw new ValidationException("El usuario que aplica el ajuste debe ser válido");

        AdjustmentAmount = amount;
        AdjustmentReason = reason;
        AdjustmentDate = DateTime.Now;

        // Recalcular diferencia final
        FinalDifference = CalculateDifferenceWithAdjustment(InitialDifference, AdjustmentAmount);

        // Evento de dominio
        RaiseDomainEvent(new CashClosingAdjusted(
            Id.Value,
            amount.Amount,
            reason,
            adjustedBy,
            AdjustmentDate.Value
        ));
    }

    /// <summary>
    /// Asigna el ID después de persistir (usado por Repository)
    /// </summary>
    internal void SetId(CashClosingId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido asignado");

        Id = id;
    }

    /// <summary>
    /// Asigna el folio (usado por Repository)
    /// </summary>
    internal void SetFolio(CashClosingFolio folio)
    {
        Folio = folio;
    }

    /// <summary>
    /// Establece el número de transacciones (usado por Repository)
    /// </summary>
    internal void SetTransactionCount(int count)
    {
        TransactionCount = count;
    }

    #endregion
}
