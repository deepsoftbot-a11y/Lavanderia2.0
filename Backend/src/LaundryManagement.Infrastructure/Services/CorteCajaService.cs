using System.Data.Common;
using Dapper;
using LaundryManagement.Application.DTOs.Cortes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Infrastructure.Services;

public class CorteCajaService : ICorteCajaService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CorteCajaService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RegistrarCorteCajaResponse> RegistrarCorteCajaAsync(RegistrarCorteCajaRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            // Calcular totales esperados por método de pago en el rango del turno
            const string totalesSql = """
                SELECT
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" = 'Efectivo' THEN pd."MontoPagado" ELSE 0 END), 0)       AS TotalEfectivo,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" = 'Tarjeta' THEN pd."MontoPagado" ELSE 0 END), 0)        AS TotalTarjeta,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" = 'Transferencia' THEN pd."MontoPagado" ELSE 0 END), 0)  AS TotalTransferencia,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" NOT IN ('Efectivo','Tarjeta','Transferencia') THEN pd."MontoPagado" ELSE 0 END), 0) AS TotalOtros,
                    COUNT(DISTINCT p."PagoID") AS NumeroTransacciones
                FROM "Pagos" p
                INNER JOIN "PagosDetalle" pd ON pd."PagoID" = p."PagoID"
                INNER JOIN "MetodosPago" mp ON mp."MetodoPagoID" = pd."MetodoPagoID"
                WHERE p."FechaPago" >= @FechaInicio
                  AND p."FechaPago" <= @FechaFin
                  AND p."RecibioPor" = @CajeroID
                  AND p."CanceladoEn" IS NULL
                """;

            var totales = await connection.QueryFirstAsync<TotalesDia>(
                totalesSql,
                new { request.FechaInicio, request.FechaFin, request.CajeroID });

            var totalEsperado  = totales.TotalEfectivo + totales.TotalTarjeta + totales.TotalTransferencia + totales.TotalOtros;
            var totalDeclarado = request.TotalDeclaradoEfectivo + request.TotalDeclaradoTarjeta + request.TotalDeclaradoTransferencia + request.TotalDeclaradoOtros;

            var difInicial       = totalDeclarado - totalEsperado;
            var difEfectivo      = request.TotalDeclaradoEfectivo - totales.TotalEfectivo;
            var difTarjeta       = request.TotalDeclaradoTarjeta - totales.TotalTarjeta;
            var difTransferencia = request.TotalDeclaradoTransferencia - totales.TotalTransferencia;
            var difOtros         = request.TotalDeclaradoOtros - totales.TotalOtros;

            // Generar folio
            var fecha = DateTime.Today.ToString("yyyyMMdd");
            var lastFolio = await connection.QueryFirstOrDefaultAsync<string>(
                $"""SELECT "FolioCorte" FROM "CortesCaja" WHERE "FolioCorte" LIKE 'COR-{fecha}-%' ORDER BY "FolioCorte" DESC LIMIT 1""");
            var seq = 1;
            if (lastFolio != null && int.TryParse(lastFolio[^4..], out int n)) seq = n + 1;
            var folio = $"COR-{fecha}-{seq:D4}";

            const string insertSql = """
                INSERT INTO "CortesCaja" (
                    "FolioCorte", "CajeroID", "FechaInicio", "FechaFin", "FechaCorte",
                    "TurnoDescripcion", "Observaciones",
                    "TotalEsperadoEfectivo", "TotalEsperadoTarjeta", "TotalEsperadoTransferencia", "TotalEsperadoOtros", "TotalEsperado",
                    "TotalDeclaradoEfectivo", "TotalDeclaradoTarjeta", "TotalDeclaradoTransferencia", "TotalDeclaradoOtros", "TotalDeclarado",
                    "MontoAjuste", "DiferenciaInicial", "DiferenciaFinal",
                    "DiferenciaInicialEfectivo", "DiferenciaInicialTarjeta", "DiferenciaInicialTransferencia", "DiferenciaInicialOtros",
                    "NumeroTransacciones"
                )
                VALUES (
                    @FolioCorte, @CajeroID, @FechaInicio, @FechaFin, NOW(),
                    @TurnoDescripcion, @Observaciones,
                    @TotalEsperadoEfectivo, @TotalEsperadoTarjeta, @TotalEsperadoTransferencia, @TotalEsperadoOtros, @TotalEsperado,
                    @TotalDeclaradoEfectivo, @TotalDeclaradoTarjeta, @TotalDeclaradoTransferencia, @TotalDeclaradoOtros, @TotalDeclarado,
                    0, @DiferenciaInicial, @DiferenciaInicial,
                    @DifEfectivo, @DifTarjeta, @DifTransferencia, @DifOtros,
                    @NumeroTransacciones
                )
                RETURNING "CorteID"
                """;

            var corteId = await connection.QuerySingleAsync<int>(insertSql, new
            {
                FolioCorte                 = folio,
                request.CajeroID,
                request.FechaInicio,
                request.FechaFin,
                request.TurnoDescripcion,
                request.Observaciones,
                TotalEsperadoEfectivo      = totales.TotalEfectivo,
                TotalEsperadoTarjeta       = totales.TotalTarjeta,
                TotalEsperadoTransferencia = totales.TotalTransferencia,
                TotalEsperadoOtros         = totales.TotalOtros,
                TotalEsperado              = totalEsperado,
                request.TotalDeclaradoEfectivo,
                request.TotalDeclaradoTarjeta,
                request.TotalDeclaradoTransferencia,
                request.TotalDeclaradoOtros,
                TotalDeclarado             = totalDeclarado,
                DiferenciaInicial          = difInicial,
                DifEfectivo                = difEfectivo,
                DifTarjeta                 = difTarjeta,
                DifTransferencia           = difTransferencia,
                DifOtros                   = difOtros,
                NumeroTransacciones        = (int)totales.NumeroTransacciones
            });

            return new RegistrarCorteCajaResponse { CorteID = corteId };
        }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al registrar el corte de caja en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al registrar el corte de caja", ex);
        }
    }

    public async Task AjustarCorteCajaAsync(AjustarCorteCajaRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var affected = await connection.ExecuteAsync(
                """
                UPDATE "CortesCaja"
                SET "MontoAjuste"  = @MontoAjuste,
                    "MotivoAjuste" = @Motivo,
                    "FechaAjuste"  = NOW(),
                    "DiferenciaFinal" = "DiferenciaInicial" + @MontoAjuste
                WHERE "CorteID" = @CorteID
                """,
                new { request.CorteID, request.MontoAjuste, request.Motivo });

            if (affected == 0)
                throw new NotFoundException($"Corte de caja {request.CorteID} no encontrado");
        }
        catch (NotFoundException) { throw; }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al ajustar el corte de caja en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al ajustar el corte de caja", ex);
        }
    }

    public async Task<IEnumerable<HistorialCorteResponse>> ConsultarHistorialCortesAsync(ConsultarHistorialCortesRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    cc."CorteID"          AS CorteID,
                    cc."CajeroID"         AS CajeroID,
                    COALESCE(u."NombreCompleto", u."NombreUsuario") AS NombreCajero,
                    cc."FechaInicio"      AS FechaInicio,
                    cc."FechaFin"         AS FechaFin,
                    cc."TurnoDescripcion" AS TurnoDescripcion,
                    cc."TotalDeclarado"   AS TotalDeclarado,
                    cc."TotalEsperado"    AS TotalSistema,
                    COALESCE(cc."DiferenciaFinal", 0) AS Diferencia
                FROM "CortesCaja" cc
                INNER JOIN "Usuarios" u ON u."UsuarioID" = cc."CajeroID"
                WHERE (@CajeroID IS NULL OR cc."CajeroID" = @CajeroID)
                  AND (@FechaInicio IS NULL OR cc."FechaCorte" >= @FechaInicio)
                  AND (@FechaFin IS NULL OR cc."FechaCorte" <= @FechaFin)
                  AND (@SoloConDiferencias = false
                       OR COALESCE(cc."DiferenciaFinal", 0) <> 0)
                ORDER BY cc."FechaCorte" DESC
                """;

            return await connection.QueryAsync<HistorialCorteResponse>(sql, new
            {
                request.CajeroID,
                request.FechaInicio,
                request.FechaFin,
                request.SoloConDiferencias
            });
        }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al consultar el historial de cortes en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al consultar el historial de cortes", ex);
        }
    }

    public async Task<DetalleCorteResponse> VerDetalleCorteAsync(int corteID)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    cc."CorteID"                    AS CorteID,
                    cc."FechaInicio"                AS FechaInicio,
                    cc."FechaFin"                   AS FechaFin,
                    cc."TurnoDescripcion"           AS TurnoDescripcion,
                    cc."TotalDeclaradoEfectivo"     AS TotalDeclaradoEfectivo,
                    cc."TotalDeclaradoTarjeta"      AS TotalDeclaradoTarjeta,
                    cc."TotalDeclaradoTransferencia" AS TotalDeclaradoTransferencia,
                    cc."TotalDeclaradoOtros"        AS TotalDeclaradoOtros,
                    cc."TotalDeclarado"             AS TotalDeclarado,
                    cc."TotalEsperadoEfectivo"      AS TotalSistemaEfectivo,
                    cc."TotalEsperadoTarjeta"       AS TotalSistemaTarjeta,
                    cc."TotalEsperadoTransferencia" AS TotalSistemaTransferencia,
                    cc."TotalEsperadoOtros"         AS TotalSistemaOtros,
                    cc."TotalEsperado"              AS TotalSistema,
                    COALESCE(cc."DiferenciaFinal", 0) AS DiferenciaFinal,
                    cc."MontoAjuste"                AS MontoAjuste,
                    cc."MotivoAjuste"               AS MotivoAjuste,
                    cc."Observaciones"              AS Observaciones,
                    COALESCE(u."NombreCompleto", u."NombreUsuario") AS NombreCajero
                FROM "CortesCaja" cc
                INNER JOIN "Usuarios" u ON u."UsuarioID" = cc."CajeroID"
                WHERE cc."CorteID" = @CorteID
                """;

            var result = await connection.QueryFirstOrDefaultAsync<DetalleCorteResponse>(
                sql, new { CorteID = corteID });

            return result ?? new DetalleCorteResponse();
        }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al consultar el detalle del corte en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al consultar el detalle del corte", ex);
        }
    }

    private sealed class TotalesDia
    {
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalOtros { get; set; }
        public long NumeroTransacciones { get; set; }
    }
}
