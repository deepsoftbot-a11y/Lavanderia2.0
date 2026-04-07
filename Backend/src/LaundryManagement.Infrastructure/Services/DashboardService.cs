using System.Data;
using Dapper;
using LaundryManagement.Application.DTOs;
using LaundryManagement.Application.Interfaces;

namespace LaundryManagement.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDbConnectionFactory _db;

    public DashboardService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetDashboardAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();

        var kpis = new DashboardKPIsDto
        {
            IngresosTotales         = await GetIngresosTotales(conn, fechaInicio, fechaFin),
            TicketPromedio          = await GetTicketPromedio(conn, fechaInicio, fechaFin),
            TotalDescuentos         = await GetTotalDescuentos(conn, fechaInicio, fechaFin),
            IngresosPorMetodo       = await GetIngresosPorMetodo(conn, fechaInicio, fechaFin),
            OrdenesAtrasadas        = await GetOrdenesAtrasadas(conn),
            OrdenesPendientesPagar  = await GetOrdenesPendientesPagar(conn, fechaInicio, fechaFin),
            ClientesNuevos          = await GetClientesNuevos(conn, fechaInicio, fechaFin),
            ClienteTop              = await GetClienteTop(conn, fechaInicio, fechaFin),
            TotalCorteCaja          = await GetTotalCorteCaja(conn, fechaInicio, fechaFin),
            Diferencias             = await GetDiferencias(conn, fechaInicio, fechaFin),
            Transacciones           = await GetTransacciones(conn, fechaInicio, fechaFin),
        };

        var charts = new DashboardChartsDto
        {
            IngresosPorDia      = await GetIngresosPorDia(conn, fechaInicio, fechaFin),
            OrdenesPorEstado    = await GetOrdenesPorEstado(conn, fechaInicio, fechaFin),
            IngresosPorServicio = await GetIngresosPorServicio(conn, fechaInicio, fechaFin),
            IngresosPorCategoria = await GetIngresosPorCategoria(conn, fechaInicio, fechaFin),
            ComparativaSemanal  = await GetComparativaSemanal(conn),
        };

        return new DashboardDto { Kpis = kpis, Charts = charts };
    }

    private static async Task<decimal> GetIngresosTotales(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""Total""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<decimal> GetTicketPromedio(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(AVG(""Total""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<decimal> GetTotalDescuentos(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""Descuento""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<List<IngresoPorMetodoDto>> GetIngresosPorMetodo(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT m.""NombreMetodo"" AS Metodo, COALESCE(SUM(pd.""MontoPagado""), 0) AS Total
                    FROM ""Pagos"" p
                    JOIN ""PagosDetalle"" pd ON p.""PagoID"" = pd.""PagoID""
                    JOIN ""MetodosPago"" m ON pd.""MetodoPagoID"" = m.""MetodoPagoID""
                    JOIN ""Ordenes"" o ON p.""OrdenID"" = o.""OrdenID""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY m.""NombreMetodo""";
        var result = await conn.QueryAsync<IngresoPorMetodoDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private static async Task<int> GetOrdenesAtrasadas(IDbConnection conn)
    {
        var sql = @"SELECT COUNT(*) FROM ""Ordenes"" o
                    WHERE o.""FechaPrometida"" < CURRENT_DATE
                    AND o.""EstadoOrdenID"" NOT IN (4, 5)
                    AND o.""FechaEntrega"" IS NULL";
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    private static async Task<OrdenesPendientesPagarDto> GetOrdenesPendientesPagar(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COUNT(*) AS Cantidad, COALESCE(SUM(o.""Total""), 0) AS Total
                    FROM ""Ordenes"" o
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    AND o.""Total"" > COALESCE((
                        SELECT SUM(p.""MontoPago"") FROM ""Pagos"" p
                        WHERE p.""OrdenID"" = o.""OrdenID"" AND p.""CanceladoEn"" IS NULL
                    ), 0)";
        var row = await conn.QuerySingleAsync<(int Cantidad, decimal Total)>(sql, new { Fi = fi, Ff = ff });
        return new OrdenesPendientesPagarDto { Cantidad = row.Cantidad, Total = row.Total };
    }

    private static async Task<int> GetClientesNuevos(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COUNT(*) FROM ""Clientes""
                    WHERE ""FechaRegistro"" >= @Fi AND ""FechaRegistro"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<ClienteTopDto?> GetClienteTop(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT c.""NombreCompleto"" AS Nombre, COUNT(*) AS Ordenes
                    FROM ""Ordenes"" o
                    JOIN ""Clientes"" c ON o.""ClienteID"" = c.""ClienteID""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY c.""ClienteID"", c.""NombreCompleto""
                    ORDER BY COUNT(*) DESC
                    LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<ClienteTopDto>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<decimal> GetTotalCorteCaja(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""TotalDeclarado""), 0) FROM ""CortesCaja""
                    WHERE ""FechaCorte"" >= @Fi AND ""FechaCorte"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<int> GetDiferencias(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COUNT(*) FROM ""CortesCaja""
                    WHERE ""FechaCorte"" >= @Fi AND ""FechaCorte"" < @Ff + INTERVAL '1 day'
                    AND ""DiferenciaFinal"" != 0";
        return await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<int> GetTransacciones(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""NumeroTransacciones""), 0) FROM ""CortesCaja""
                    WHERE ""FechaCorte"" >= @Fi AND ""FechaCorte"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });
    }

    private static async Task<List<IngresoPorDiaDto>> GetIngresosPorDia(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT DATE(""FechaRecepcion"") AS Fecha,
                           COALESCE(SUM(""Total""), 0) AS Ingresos,
                           COUNT(*) AS Ordenes
                    FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY DATE(""FechaRecepcion"")
                    ORDER BY Fecha";
        var result = await conn.QueryAsync<IngresoPorDiaDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private static async Task<List<OrdenesPorEstadoDto>> GetOrdenesPorEstado(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT e.""NombreEstado"" AS Estado, COUNT(*) AS Cantidad
                    FROM ""Ordenes"" o
                    JOIN ""EstadosOrden"" e ON o.""EstadoOrdenID"" = e.""EstadoOrdenID""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY e.""NombreEstado"", e.""OrdenProceso""
                    ORDER BY e.""OrdenProceso""";
        var result = await conn.QueryAsync<OrdenesPorEstadoDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private static async Task<List<IngresoPorServicioDto>> GetIngresosPorServicio(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT s.""NombreServicio"" AS Servicio,
                           COALESCE(SUM(od.""Subtotal""), 0) AS Total
                    FROM ""OrdenesDetalle"" od
                    JOIN ""Ordenes"" o ON od.""OrdenID"" = o.""OrdenID""
                    JOIN ""Servicios"" s ON od.""ServicioID"" = s.""ServicioID""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY s.""NombreServicio""
                    ORDER BY Total DESC";
        var result = await conn.QueryAsync<IngresoPorServicioDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private static async Task<List<IngresoPorCategoriaDto>> GetIngresosPorCategoria(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT c.""NombreCategoria"" AS Categoria,
                           COALESCE(SUM(od.""Subtotal""), 0) AS Total
                    FROM ""OrdenesDetalle"" od
                    JOIN ""Ordenes"" o ON od.""OrdenID"" = o.""OrdenID""
                    JOIN ""Servicios"" s ON od.""ServicioID"" = s.""ServicioID""
                    JOIN ""Categorias"" c ON s.""CategoriaID"" = c.""CategoriaID""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY c.""NombreCategoria""
                    ORDER BY Total DESC";
        var result = await conn.QueryAsync<IngresoPorCategoriaDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private static async Task<ComparativaSemanalDto> GetComparativaSemanal(IDbConnection conn)
    {
        var hoy = DateTime.Today;
        var inicioSemanaActual  = hoy.AddDays(-(int)hoy.DayOfWeek);
        var finSemanaActual     = inicioSemanaActual.AddDays(6);
        var inicioSemanaAnterior = inicioSemanaActual.AddDays(-7);
        var finSemanaAnterior   = inicioSemanaActual.AddDays(-1);

        var sql = @"SELECT COALESCE(SUM(""Total""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" <= @Ff + INTERVAL '1 day'";

        var actual   = await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = inicioSemanaActual,   Ff = finSemanaActual });
        var anterior = await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = inicioSemanaAnterior, Ff = finSemanaAnterior });

        return new ComparativaSemanalDto { SemanaActual = actual, SemanaAnterior = anterior };
    }
}
