using ClosedXML.Excel;
using LaundryManagement.Application.DTOs.Reports;
using LaundryManagement.Application.Interfaces;

namespace LaundryManagement.Infrastructure.Reports;

public class ExcelReportGenerator : IReportFileGenerator
{
    public string Formato => "EXCEL";

    public Task<byte[]> GenerateAsync(CashClosingReportData data, int corteId, DateTime fecha)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Corte de Caja");

        var row = 1;

        // Título
        ws.Cell(row, 1).Value = $"CORTE DE CAJA — {data.Cashier.Nombre}";
        ws.Range(row, 1, row, 4).Merge().Style.Font.Bold = true;
        ws.Range(row, 1, row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;

        ws.Cell(row, 1).Value = $"Turno: {data.Cashier.Turno}  |  Fecha: {data.Cashier.Fecha:dd/MM/yyyy}";
        ws.Range(row, 1, row, 4).Merge();
        row += 2;

        // Sección VENTAS DEL DÍA
        row = WriteSection(ws, row, "VENTAS DEL DÍA", new[]
        {
            ("TOTAL EFECTIVO",       data.VentasDia.Efectivo,       0m,                        data.VentasDia.Efectivo),
            ("TOTAL TARJETA",        data.VentasDia.Tarjeta,        0m,                        data.VentasDia.Efectivo + data.VentasDia.Tarjeta),
            ("TOTAL TRANSFERENCIA",  data.VentasDia.Transferencia,  0m,                        data.VentasDia.Efectivo + data.VentasDia.Tarjeta + data.VentasDia.Transferencia),
            ("TOTAL OTROS",          data.VentasDia.Otros,          0m,                        data.VentasDia.TotalVentas),
            ("TOTAL VENTAS DEL DÍA", 0m,                            data.VentasDia.TotalVentas, 0m),
        });

        row++;

        // Sección BALANZA
        row = WriteSection(ws, row, "BALANZA \"PENDIENTE DE COBRO\"", new[]
        {
            ("PENDIENTE POR COBRAR HOY", data.Balanza.PendienteHoy,  0m,                         data.Balanza.PendienteHoy),
            ("ACUMULADO AL DÍA DE AYER", data.Balanza.AcumuladoAyer, 0m,                         data.Balanza.NuevoAcumulado),
            ("NUEVO ACUMULADO A HOY",    0m,                          data.Balanza.NuevoAcumulado, 0m),
        });

        row++;

        // Sección CORTE DE CAJA CHICA
        WriteSection(ws, row, "CORTE DE CAJA CHICA", new[]
        {
            ("FONDO INICIAL",          data.CorteCaja.FondoInicial,    0m,                           data.CorteCaja.FondoInicial),
            ("CORTE SISTEMA EFECTIVO", data.CorteCaja.SistemaEfectivo, 0m,                           data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo),
            ("RETIRO EFECTIVO",        0m,                              data.CorteCaja.RetiroEfectivo, data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo - data.CorteCaja.RetiroEfectivo),
            ("AJUSTE DE CAJA",         data.CorteCaja.AjusteCaja,      0m,                           data.CorteCaja.FondoFinal),
            ("FONDO FINAL",            0m,                              0m,                           data.CorteCaja.FondoFinal),
        });

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    private static int WriteSection(IXLWorksheet ws, int row,
        string title,
        IEnumerable<(string Concepto, decimal Cargo, decimal Abono, decimal Saldo)> rows)
    {
        // Section title
        ws.Cell(row, 1).Value = title;
        ws.Range(row, 1, row, 4).Merge().Style.Font.Bold = true;
        ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;

        // Header
        ws.Cell(row, 1).Value = "CONCEPTO";
        ws.Cell(row, 2).Value = "CARGO";
        ws.Cell(row, 3).Value = "ABONO";
        ws.Cell(row, 4).Value = "SALDO";
        var headerRange = ws.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        row++;

        // Data rows
        foreach (var (concepto, cargo, abono, saldo) in rows)
        {
            ws.Cell(row, 1).Value = concepto;

            if (cargo > 0) { ws.Cell(row, 2).Value = cargo; ws.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00"; }
            else ws.Cell(row, 2).Value = "-";

            if (abono > 0) { ws.Cell(row, 3).Value = abono; ws.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00"; }
            else ws.Cell(row, 3).Value = "-";

            if (saldo > 0) { ws.Cell(row, 4).Value = saldo; ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00"; }
            else ws.Cell(row, 4).Value = "-";

            row++;
        }

        return row;
    }
}
