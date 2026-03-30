using System.Data.Common;
using Dapper;
using LaundryManagement.Application.DTOs.Reportes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReporteService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<VentaDiariaResponse> ReporteVentasDiariasAsync(DateTime fecha)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    @Fecha AS Fecha,
                    COUNT(DISTINCT o."OrdenID") AS TotalOrdenes,
                    COALESCE(SUM(o."Total"), 0) AS TotalVentas,
                    COALESCE(SUM(pagado."TotalPagado"), 0) AS TotalPagado,
                    COALESCE(SUM(o."Total") - SUM(COALESCE(pagado."TotalPagado", 0)), 0) AS SaldoPendiente,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" = 'Efectivo'      THEN pd."MontoPagado" ELSE 0 END), 0) AS TotalEfectivo,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" = 'Tarjeta'       THEN pd."MontoPagado" ELSE 0 END), 0) AS TotalTarjeta,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" = 'Transferencia' THEN pd."MontoPagado" ELSE 0 END), 0) AS TotalTransferencia,
                    COALESCE(SUM(CASE WHEN mp."NombreMetodo" NOT IN ('Efectivo','Tarjeta','Transferencia') THEN pd."MontoPagado" ELSE 0 END), 0) AS TotalOtros
                FROM "Ordenes" o
                LEFT JOIN (
                    SELECT p."OrdenID", SUM(p."MontoPago") AS "TotalPagado"
                    FROM "Pagos" p
                    WHERE p."CanceladoEn" IS NULL
                    GROUP BY p."OrdenID"
                ) pagado ON pagado."OrdenID" = o."OrdenID"
                LEFT JOIN "Pagos" p ON p."OrdenID" = o."OrdenID" AND p."CanceladoEn" IS NULL
                LEFT JOIN "PagosDetalle" pd ON pd."PagoID" = p."PagoID"
                LEFT JOIN "MetodosPago" mp ON mp."MetodoPagoID" = pd."MetodoPagoID"
                WHERE o."FechaRecepcion"::date = @Fecha
                """;

            var result = await connection.QueryFirstOrDefaultAsync<VentaDiariaResponse>(
                sql, new { Fecha = fecha.Date });

            return result ?? new VentaDiariaResponse { Fecha = fecha.Date };
        }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al generar el reporte de ventas diarias en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al generar el reporte de ventas diarias", ex);
        }
    }
}
