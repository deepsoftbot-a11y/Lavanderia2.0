using System.Data;
using Dapper;
using LaundryManagement.Application.DTOs.Reports;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace LaundryManagement.Infrastructure.Services;

public class ReportDataService : IReportDataService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReportDataService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CashClosingReportData> GetCashClosingReportDataAsync(
        int corteId, DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var corte = await GetCorteAsync(connection, corteId);
            if (corte is null)
                throw new NotFoundException($"Corte de caja {corteId} no encontrado");

            var balanza = await GetBalanzaAsync(connection, fechaInicio.Date);

            var fondoInicial = corte.FondoInicial ?? 0m;
            var sistemaEfect = corte.TotalEsperadoEfectivo;
            var retiroEfect  = corte.TotalDeclaradoEfectivo;
            var ajuste       = corte.MontoAjuste;
            var fondoFinal   = fondoInicial + sistemaEfect - retiroEfect + ajuste;

            return new CashClosingReportData(
                Cashier: new CashierInfo(
                    Nombre: corte.CajeroNombre,
                    Turno:  corte.TurnoDescripcion ?? string.Empty,
                    Fecha:  corte.FechaCorte),
                VentasDia: new VentasDiaSection(
                    Efectivo:      corte.TotalEsperadoEfectivo,
                    Tarjeta:       corte.TotalEsperadoTarjeta,
                    Transferencia: corte.TotalEsperadoTransferencia,
                    Otros:         corte.TotalEsperadoOtros,
                    TotalVentas:   corte.TotalEsperado),
                Balanza: new BalanzaSection(
                    PendienteHoy:   balanza.PendienteHoy,
                    AcumuladoAyer:  balanza.AcumuladoAyer,
                    NuevoAcumulado: balanza.PendienteHoy + balanza.AcumuladoAyer),
                CorteCaja: new CorteCajaSection(
                    FondoInicial:    fondoInicial,
                    SistemaEfectivo: sistemaEfect,
                    RetiroEfectivo:  retiroEfect,
                    AjusteCaja:      ajuste,
                    FondoFinal:      fondoFinal)
            );
        }
        catch (NotFoundException) { throw; }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al obtener datos del reporte de corte de caja", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al obtener datos del reporte", ex);
        }
    }

    private async Task<CorteRaw?> GetCorteAsync(IDbConnection connection, int corteId)
    {
        const string sql = """
            SELECT
                cc.CorteID,
                cc.TurnoDescripcion,
                cc.FechaInicio,
                cc.FechaFin,
                cc.FechaCorte,
                cc.FondoInicial,
                cc.TotalEsperadoEfectivo,
                cc.TotalEsperadoTarjeta,
                cc.TotalEsperadoTransferencia,
                cc.TotalEsperadoOtros,
                cc.TotalEsperado,
                cc.TotalDeclaradoEfectivo,
                cc.MontoAjuste,
                ISNULL(u.NombreCompleto, u.NombreUsuario) AS CajeroNombre
            FROM CortesCaja cc
            INNER JOIN Usuarios u ON u.UsuarioID = cc.CajeroID
            WHERE cc.CorteID = @CorteId
            """;

        return await connection.QueryFirstOrDefaultAsync<CorteRaw>(sql, new { CorteId = corteId });
    }

    private async Task<(decimal PendienteHoy, decimal AcumuladoAyer)> GetBalanzaAsync(
        IDbConnection connection, DateTime fechaHoy)
    {
        const string sql = """
            SELECT
                SUM(CASE WHEN CAST(o.FechaRecepcion AS DATE) = @FechaHoy
                         THEN o.Total - ISNULL(pagado.TotalPagado, 0) ELSE 0 END) AS PendienteHoy,
                SUM(CASE WHEN CAST(o.FechaRecepcion AS DATE) < @FechaHoy
                         THEN o.Total - ISNULL(pagado.TotalPagado, 0) ELSE 0 END) AS AcumuladoAyer
            FROM Ordenes o
            LEFT JOIN (
                SELECT p.OrdenID, SUM(p.MontoPago) AS TotalPagado
                FROM Pagos p
                WHERE p.CanceladoEn IS NULL
                GROUP BY p.OrdenID
            ) pagado ON pagado.OrdenID = o.OrdenID
            WHERE o.EstadoOrdenID != 5
              AND o.Total - ISNULL(pagado.TotalPagado, 0) > 0
            """;

        var result = await connection.QueryFirstOrDefaultAsync(sql, new { FechaHoy = fechaHoy });
        decimal pendienteHoy  = result?.PendienteHoy  ?? 0m;
        decimal acumuladoAyer = result?.AcumuladoAyer ?? 0m;
        return (pendienteHoy, acumuladoAyer);
    }

    private sealed class CorteRaw
    {
        public int      CorteId                    { get; set; }
        public string?  TurnoDescripcion           { get; set; }
        public DateTime FechaInicio                { get; set; }
        public DateTime FechaFin                   { get; set; }
        public DateTime FechaCorte                 { get; set; }
        public decimal? FondoInicial               { get; set; }
        public decimal  TotalEsperadoEfectivo      { get; set; }
        public decimal  TotalEsperadoTarjeta       { get; set; }
        public decimal  TotalEsperadoTransferencia { get; set; }
        public decimal  TotalEsperadoOtros         { get; set; }
        public decimal  TotalEsperado              { get; set; }
        public decimal  TotalDeclaradoEfectivo     { get; set; }
        public decimal  MontoAjuste                { get; set; }
        public string   CajeroNombre               { get; set; } = string.Empty;
    }
}
