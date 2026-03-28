using LaundryManagement.Domain.Aggregates.CashClosings;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper que traduce entre entidades de dominio (CashClosingPure) y entidades de infraestructura (CortesCaja).
/// Esta es la capa de anti-corrupción que mantiene el dominio puro independiente de la base de datos.
/// </summary>
public static class CashClosingMapper
{
    /// <summary>
    /// Mapea de entidad de dominio a entidad de infraestructura
    /// </summary>
    public static CortesCaja ToInfrastructure(CashClosingPure cashClosing)
    {
        var corteEntity = new CortesCaja
        {
            // NO asignar CorteId si es 0 (nuevo corte) - dejar que SQL Server genere el IDENTITY
            // Folio es asignado por el repositorio antes de mapear
            FolioCorte = cashClosing.Folio?.Value ?? string.Empty,
            CajeroId = cashClosing.CashierId,
            TurnoDescripcion = cashClosing.ShiftDescription,
            FechaInicio = cashClosing.StartDate,
            FechaFin = cashClosing.EndDate,
            FechaCorte = cashClosing.ClosingDate,
            FondoInicial = cashClosing.InitialFund?.Amount,

            // Totales esperados
            TotalEsperadoEfectivo = cashClosing.ExpectedCash.Amount,
            TotalEsperadoTarjeta = cashClosing.ExpectedCard.Amount,
            TotalEsperadoTransferencia = cashClosing.ExpectedTransfer.Amount,
            TotalEsperadoOtros = cashClosing.ExpectedOther.Amount,
            TotalEsperado = cashClosing.TotalExpected.Amount,

            // Totales declarados
            TotalDeclaradoEfectivo = cashClosing.DeclaredCash.Amount,
            TotalDeclaradoTarjeta = cashClosing.DeclaredCard.Amount,
            TotalDeclaradoTransferencia = cashClosing.DeclaredTransfer.Amount,
            TotalDeclaradoOtros = cashClosing.DeclaredOther.Amount,
            TotalDeclarado = cashClosing.TotalDeclared.Amount,

            // Diferencias iniciales (computed columns - EF Core las ignora en INSERT)
            DiferenciaInicialEfectivo = cashClosing.InitialDifferenceCash.Amount,
            DiferenciaInicialTarjeta = cashClosing.InitialDifferenceCard.Amount,
            DiferenciaInicialTransferencia = cashClosing.InitialDifferenceTransfer.Amount,
            DiferenciaInicialOtros = cashClosing.InitialDifferenceOther.Amount,
            DiferenciaInicial = cashClosing.InitialDifference.Amount,

            // Ajuste
            MontoAjuste = cashClosing.AdjustmentAmount.Amount,
            MotivoAjuste = cashClosing.AdjustmentReason,
            FechaAjuste = cashClosing.AdjustmentDate,

            // Diferencia final (computed column - EF Core la ignora en INSERT)
            DiferenciaFinal = cashClosing.FinalDifference.Amount,

            // Otros
            NumeroTransacciones = cashClosing.TransactionCount,
            Observaciones = cashClosing.Notes,

            // Detalles por método de pago
            CortesCajaDetalles = cashClosing.Details.Select(d => new CortesCajaDetalle
            {
                MetodoPagoId = d.PaymentMethodId,
                NumeroTransacciones = d.TransactionCount,
                TotalEsperado = d.ExpectedTotal.Amount,
                TotalDeclarado = d.DeclaredTotal.Amount
                // Diferencia es computed column en BD
            }).ToList()
        };

        // Solo asignar CorteId si ya existe (no es un nuevo corte)
        if (!cashClosing.Id.IsEmpty)
        {
            corteEntity.CorteId = cashClosing.Id.Value;
        }

        return corteEntity;
    }

    /// <summary>
    /// Mapea de entidad de infraestructura a entidad de dominio
    /// </summary>
    public static CashClosingPure ToDomain(CortesCaja corteEntity)
    {
        return CashClosingPure.Reconstitute(
            id: CashClosingId.From(corteEntity.CorteId),
            folio: CashClosingFolio.FromString(corteEntity.FolioCorte),
            cashierId: corteEntity.CajeroId,
            shiftDescription: corteEntity.TurnoDescripcion,
            startDate: corteEntity.FechaInicio,
            endDate: corteEntity.FechaFin,
            closingDate: corteEntity.FechaCorte,

            // Totales esperados
            expectedCash: Money.FromDecimal(corteEntity.TotalEsperadoEfectivo),
            expectedCard: Money.FromDecimal(corteEntity.TotalEsperadoTarjeta),
            expectedTransfer: Money.FromDecimal(corteEntity.TotalEsperadoTransferencia),
            expectedOther: Money.FromDecimal(corteEntity.TotalEsperadoOtros),
            totalExpected: Money.FromDecimal(corteEntity.TotalEsperado),

            // Totales declarados
            declaredCash: Money.FromDecimal(corteEntity.TotalDeclaradoEfectivo),
            declaredCard: Money.FromDecimal(corteEntity.TotalDeclaradoTarjeta),
            declaredTransfer: Money.FromDecimal(corteEntity.TotalDeclaradoTransferencia),
            declaredOther: Money.FromDecimal(corteEntity.TotalDeclaradoOtros),
            totalDeclared: Money.FromDecimal(corteEntity.TotalDeclarado),

            // Diferencias iniciales (MoneyDifference permite negativos)
            initialDifferenceCash: MoneyDifference.FromDecimal(corteEntity.DiferenciaInicialEfectivo ?? 0),
            initialDifferenceCard: MoneyDifference.FromDecimal(corteEntity.DiferenciaInicialTarjeta ?? 0),
            initialDifferenceTransfer: MoneyDifference.FromDecimal(corteEntity.DiferenciaInicialTransferencia ?? 0),
            initialDifferenceOther: MoneyDifference.FromDecimal(corteEntity.DiferenciaInicialOtros ?? 0),
            initialDifference: MoneyDifference.FromDecimal(corteEntity.DiferenciaInicial ?? 0),

            // Ajuste
            adjustmentAmount: Money.FromDecimal(corteEntity.MontoAjuste),
            adjustmentReason: corteEntity.MotivoAjuste,
            adjustmentDate: corteEntity.FechaAjuste,

            // Diferencia final (MoneyDifference permite negativos)
            finalDifference: MoneyDifference.FromDecimal(corteEntity.DiferenciaFinal ?? 0),

            // Otros
            transactionCount: corteEntity.NumeroTransacciones,
            notes: corteEntity.Observaciones,

            // Fondo inicial
            initialFund: corteEntity.FondoInicial.HasValue
                ? Money.FromDecimal(corteEntity.FondoInicial.Value) : null,

            // Detalles por método de pago
            details: corteEntity.CortesCajaDetalles.Select(d =>
                CashClosingDetail.Reconstitute(
                    d.CorteDetalleId,
                    d.MetodoPagoId,
                    d.NumeroTransacciones,
                    Money.FromDecimal(d.TotalEsperado),
                    Money.FromDecimal(d.TotalDeclarado),
                    MoneyDifference.FromDecimal(d.Diferencia ?? 0))
            ).ToList()
        );
    }
}
