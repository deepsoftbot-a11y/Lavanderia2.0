using LaundryManagement.Application.DTOs.Reports;
using LaundryManagement.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LaundryManagement.Infrastructure.Reports;

public class PdfReportGenerator : IReportFileGenerator
{
    public string Formato => "PDF";

    public Task<byte[]> GenerateAsync(CashClosingReportData data, int corteId, DateTime fecha)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text($"CORTE DE CAJA — {data.Cashier.Nombre}")
                        .Bold().FontSize(14).AlignCenter();
                    col.Item().Text($"Turno: {data.Cashier.Turno}  |  Fecha: {data.Cashier.Fecha:dd/MM/yyyy}")
                        .FontSize(9).AlignCenter();
                    col.Item().PaddingVertical(4).LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingVertical(6)
                        .Text("VENTAS DEL DÍA").Bold().FontSize(11);
                    col.Item().Element(e => BuildTable(e, new[]
                    {
                        ("TOTAL EFECTIVO",       data.VentasDia.Efectivo,       0m,                        data.VentasDia.Efectivo),
                        ("TOTAL TARJETA",        data.VentasDia.Tarjeta,        0m,                        data.VentasDia.Efectivo + data.VentasDia.Tarjeta),
                        ("TOTAL TRANSFERENCIA",  data.VentasDia.Transferencia,  0m,                        data.VentasDia.Efectivo + data.VentasDia.Tarjeta + data.VentasDia.Transferencia),
                        ("TOTAL OTROS",          data.VentasDia.Otros,          0m,                        data.VentasDia.TotalVentas),
                        ("TOTAL VENTAS DEL DÍA", 0m,                            data.VentasDia.TotalVentas, 0m),
                    }));

                    col.Item().PaddingTop(10)
                        .Text("BALANZA \"PENDIENTE DE COBRO\"").Bold().FontSize(11);
                    col.Item().Element(e => BuildTable(e, new[]
                    {
                        ("PENDIENTE POR COBRAR HOY", data.Balanza.PendienteHoy,  0m,                         data.Balanza.PendienteHoy),
                        ("ACUMULADO AL DÍA DE AYER", data.Balanza.AcumuladoAyer, 0m,                         data.Balanza.NuevoAcumulado),
                        ("NUEVO ACUMULADO A HOY",    0m,                          data.Balanza.NuevoAcumulado, 0m),
                    }));

                    col.Item().PaddingTop(10)
                        .Text("CORTE DE CAJA CHICA").Bold().FontSize(11);
                    col.Item().Element(e => BuildTable(e, new[]
                    {
                        ("FONDO INICIAL",          data.CorteCaja.FondoInicial,    0m,                           data.CorteCaja.FondoInicial),
                        ("CORTE SISTEMA EFECTIVO", data.CorteCaja.SistemaEfectivo, 0m,                           data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo),
                        ("RETIRO EFECTIVO",        0m,                              data.CorteCaja.RetiroEfectivo, data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo - data.CorteCaja.RetiroEfectivo),
                        ("AJUSTE DE CAJA",         data.CorteCaja.AjusteCaja,      0m,                           data.CorteCaja.FondoFinal),
                        ("FONDO FINAL",            0m,                              0m,                           data.CorteCaja.FondoFinal),
                    }));
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generado: ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
            });
        });

        var bytes = pdf.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private static void BuildTable(IContainer container,
        IEnumerable<(string Concepto, decimal Cargo, decimal Abono, decimal Saldo)> rows)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(4);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
            });

            static IContainer HeaderCell(IContainer c) =>
                c.Background(Colors.Grey.Lighten2).Padding(4);

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("CONCEPTO").Bold();
                h.Cell().Element(HeaderCell).AlignRight().Text("CARGO").Bold();
                h.Cell().Element(HeaderCell).AlignRight().Text("ABONO").Bold();
                h.Cell().Element(HeaderCell).AlignRight().Text("SALDO").Bold();
            });

            static IContainer DataCell(IContainer c) =>
                c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4);

            foreach (var (concepto, cargo, abono, saldo) in rows)
            {
                table.Cell().Element(DataCell).Text(concepto);
                table.Cell().Element(DataCell).AlignRight()
                    .Text(cargo > 0 ? cargo.ToString("C") : "-");
                table.Cell().Element(DataCell).AlignRight()
                    .Text(abono > 0 ? abono.ToString("C") : "-");
                table.Cell().Element(DataCell).AlignRight()
                    .Text(saldo > 0 ? saldo.ToString("C") : "-");
            }
        });
    }
}
