using System.Data.Common;
using ClosedXML.Excel;
using Dapper;
using LaundryManagement.Application.DTOs.Reportes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

    private async Task<List<OrdenExportRow>> GetOrdenExportRowsAsync(
        DateTime? startDate,
        DateTime? endDate,
        int[]? statusIds,
        string[]? paymentStatuses)
    {
        using var connection = _connectionFactory.CreateConnection();

        var conditions = new List<string> { "1=1" };
        var parameters = new DynamicParameters();

        if (startDate.HasValue)
        {
            conditions.Add(@"o.""FechaRecepcion"" >= @StartDate");
            parameters.Add("StartDate", startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            conditions.Add(@"o.""FechaRecepcion"" < @EndDate");
            parameters.Add("EndDate", endDate.Value.Date.AddDays(1));
        }
        if (statusIds != null && statusIds.Length > 0)
        {
            conditions.Add(@"o.""EstadoOrdenID"" = ANY(@StatusIds)");
            parameters.Add("StatusIds", statusIds);
        }

        var where = string.Join(" AND ", conditions);

        var sql = $"""
            SELECT
                o."FolioOrden"                                                                   AS "Folio",
                c."NombreCompleto"                                                               AS "Cliente",
                o."FechaRecepcion"                                                               AS "FechaCreacion",
                o."FechaPrometida"                                                               AS "FechaPrometida",
                eo."NombreEstado"                                                                AS "EstadoOrden",
                COALESCE(SUM(p."MontoPago") FILTER (WHERE p."CanceladoEn" IS NULL), 0)          AS "Pagado",
                o."Subtotal"                                                                     AS "Subtotal",
                o."Descuento"                                                                    AS "Descuento",
                o."Total"                                                                        AS "Total"
            FROM "Ordenes" o
            JOIN "Clientes" c ON c."ClienteID" = o."ClienteID"
            JOIN "EstadosOrden" eo ON eo."EstadoOrdenID" = o."EstadoOrdenID"
            LEFT JOIN "Pagos" p ON p."OrdenID" = o."OrdenID"
            WHERE {where}
            GROUP BY o."OrdenID", o."FolioOrden", c."NombreCompleto",
                     o."FechaRecepcion", o."FechaPrometida", eo."NombreEstado",
                     o."Subtotal", o."Descuento", o."Total"
            ORDER BY o."FechaRecepcion" DESC
            """;

        var rows = (await connection.QueryAsync<dynamic>(sql, parameters)).ToList();

        var result = rows.Select(r =>
        {
            decimal pagado = (decimal)r.Pagado;
            decimal total  = (decimal)r.Total;
            string estPago = pagado >= total && total > 0 ? "paid"
                           : pagado > 0 ? "partial" : "pending";
            return new OrdenExportRow(
                Folio:          (string)r.Folio,
                Cliente:        (string)r.Cliente,
                FechaCreacion:  (DateTime)r.FechaCreacion,
                FechaPrometida: (DateTime)r.FechaPrometida,
                EstadoOrden:    (string)r.EstadoOrden,
                EstadoPago:     estPago == "paid" ? "Pagado" : estPago == "partial" ? "Parcial" : "Pendiente",
                Subtotal:       (decimal)r.Subtotal,
                Descuento:      (decimal)r.Descuento,
                Total:          total,
                Pagado:         pagado,
                Saldo:          total - pagado
            );
        }).ToList();

        if (paymentStatuses != null && paymentStatuses.Length > 0)
        {
            var statusMap = new Dictionary<string, string>
            {
                ["paid"]    = "Pagado",
                ["partial"] = "Parcial",
                ["pending"] = "Pendiente"
            };
            var labels = paymentStatuses
                .Where(s => statusMap.ContainsKey(s))
                .Select(s => statusMap[s])
                .ToHashSet();
            result = result.Where(r => labels.Contains(r.EstadoPago)).ToList();
        }

        return result;
    }

    public async Task<byte[]> ExportOrdenesExcelAsync(
        DateTime? startDate,
        DateTime? endDate,
        int[]? statusIds,
        string[]? paymentStatuses)
    {
        var rows = await GetOrdenExportRowsAsync(startDate, endDate, statusIds, paymentStatuses);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Órdenes");

        string[] headers = { "Folio", "Cliente", "Fecha Creación", "Fecha Prometida",
                              "Estado Orden", "Estado Pago", "Subtotal", "Descuento",
                              "Total", "Pagado", "Saldo" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 99, 235);
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int row = i + 2;
            ws.Cell(row, 1).Value  = r.Folio;
            ws.Cell(row, 2).Value  = r.Cliente;
            ws.Cell(row, 3).Value  = r.FechaCreacion;
            ws.Cell(row, 4).Value  = r.FechaPrometida;
            ws.Cell(row, 5).Value  = r.EstadoOrden;
            ws.Cell(row, 6).Value  = r.EstadoPago;
            ws.Cell(row, 7).Value  = r.Subtotal;
            ws.Cell(row, 8).Value  = r.Descuento;
            ws.Cell(row, 9).Value  = r.Total;
            ws.Cell(row, 10).Value = r.Pagado;
            ws.Cell(row, 11).Value = r.Saldo;

            for (int col = 7; col <= 11; col++)
                ws.Cell(row, col).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportOrdenesPdfAsync(
        DateTime? startDate,
        DateTime? endDate,
        int[]? statusIds,
        string[]? paymentStatuses)
    {
        var rows = await GetOrdenExportRowsAsync(startDate, endDate, statusIds, paymentStatuses);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);

                page.Header().Text($"Historial de Órdenes — {DateTime.Today:dd/MM/yyyy}")
                    .FontSize(14).Bold().AlignCenter();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                    });

                    static IContainer HeaderCell(IContainer c) =>
                        c.Background("#2563eb").Padding(4);

                    string[] headers = { "Folio", "Cliente", "Creación", "Prometida",
                                         "Estado Orden", "Estado Pago", "Subtotal", "Descuento",
                                         "Total", "Pagado", "Saldo" };
                    table.Header(header =>
                    {
                        foreach (var h in headers)
                            header.Cell().Element(HeaderCell)
                                .Text(h).FontColor("#ffffff").FontSize(8).Bold();
                    });

                    foreach (var r in rows)
                    {
                        static IContainer DataCell(IContainer c) =>
                            c.BorderBottom(1).BorderColor("#e5e7eb").Padding(4);

                        table.Cell().Element(DataCell).Text(r.Folio).FontSize(7);
                        table.Cell().Element(DataCell).Text(r.Cliente).FontSize(7);
                        table.Cell().Element(DataCell).Text(r.FechaCreacion.ToString("dd/MM/yy")).FontSize(7);
                        table.Cell().Element(DataCell).Text(r.FechaPrometida.ToString("dd/MM/yy")).FontSize(7);
                        table.Cell().Element(DataCell).Text(r.EstadoOrden).FontSize(7);
                        table.Cell().Element(DataCell).Text(r.EstadoPago).FontSize(7);
                        table.Cell().Element(DataCell).Text($"${r.Subtotal:N2}").FontSize(7).AlignRight();
                        table.Cell().Element(DataCell).Text($"${r.Descuento:N2}").FontSize(7).AlignRight();
                        table.Cell().Element(DataCell).Text($"${r.Total:N2}").FontSize(7).Bold().AlignRight();
                        table.Cell().Element(DataCell).Text($"${r.Pagado:N2}").FontSize(7).AlignRight();
                        table.Cell().Element(DataCell).Text($"${r.Saldo:N2}").FontSize(7).AlignRight();
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ").FontSize(8);
                    x.CurrentPageNumber().FontSize(8);
                    x.Span(" de ").FontSize(8);
                    x.TotalPages().FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }
}
