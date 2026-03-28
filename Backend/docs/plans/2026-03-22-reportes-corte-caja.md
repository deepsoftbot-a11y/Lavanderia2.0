# Reportes Automáticos Post-Corte de Caja — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Al registrar un corte de caja, generar automáticamente reportes (PDF/Excel) configurados en `ConfiguracionReporte` y enviarlos por email a los destinatarios definidos.

**Architecture:** Domain event `CashClosingCreated` se publica post-commit desde `CreateCashClosingCommandHandler`. Un nuevo `INotificationHandler` escucha ese evento, lee `ConfiguracionReporte` activos con `TipoReporte="CorteCaja"`, genera el archivo en memoria con QuestPDF o ClosedXML según `FormatoExportacion`, y lo envía por SMTP. Todo fallo se registra en `HistorialReporte` y nunca propaga al caller.

**Tech Stack:** .NET 8, MediatR domain events, QuestPDF, ClosedXML, System.Net.Mail, Dapper, EF Core

---

### Task 1: Agregar paquetes NuGet a Infrastructure

**Files:**
- Modify: `src/LaundryManagement.Infrastructure/LaundryManagement.Infrastructure.csproj`

**Step 1: Agregar los paquetes**

```bash
cd src/LaundryManagement.Infrastructure
dotnet add package QuestPDF
dotnet add package ClosedXML
```

**Step 2: Verificar build**

```bash
cd ../..
dotnet build LaundryManagement.sln
```
Expected: Build succeeded, 0 errors.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Infrastructure/LaundryManagement.Infrastructure.csproj
git commit -m "chore: add QuestPDF and ClosedXML packages"
```

---

### Task 2: Crear DTOs del reporte en Application

**Files:**
- Create: `src/LaundryManagement.Application/DTOs/Reports/CashClosingReportData.cs`

**Step 1: Crear el archivo**

```csharp
namespace LaundryManagement.Application.DTOs.Reports;

public record CashClosingReportData(
    CashierInfo      Cashier,
    VentasDiaSection VentasDia,
    BalanzaSection   Balanza,
    CorteCajaSection CorteCaja
);

public record CashierInfo(
    string   Nombre,
    string   Turno,
    DateTime Fecha
);

public record VentasDiaSection(
    decimal Efectivo,
    decimal Tarjeta,
    decimal Transferencia,
    decimal Otros,
    decimal TotalVentas
);

public record BalanzaSection(
    decimal PendienteHoy,
    decimal AcumuladoAyer,
    decimal NuevoAcumulado
);

public record CorteCajaSection(
    decimal FondoInicial,
    decimal SistemaEfectivo,
    decimal RetiroEfectivo,
    decimal AjusteCaja,
    decimal FondoFinal
);
```

**Step 2: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Application/DTOs/Reports/CashClosingReportData.cs
git commit -m "feat: add CashClosingReportData DTOs"
```

---

### Task 3: Crear interfaces en Application

**Files:**
- Create: `src/LaundryManagement.Application/Interfaces/IReportDataService.cs`
- Create: `src/LaundryManagement.Application/Interfaces/IReportFileGenerator.cs`
- Create: `src/LaundryManagement.Application/Interfaces/IEmailService.cs`

**Step 1: IReportDataService**

```csharp
// src/LaundryManagement.Application/Interfaces/IReportDataService.cs
using LaundryManagement.Application.DTOs.Reports;

namespace LaundryManagement.Application.Interfaces;

public interface IReportDataService
{
    Task<CashClosingReportData> GetCashClosingReportDataAsync(
        int      corteId,
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken ct = default);
}
```

**Step 2: IReportFileGenerator**

```csharp
// src/LaundryManagement.Application/Interfaces/IReportFileGenerator.cs
using LaundryManagement.Application.DTOs.Reports;

namespace LaundryManagement.Application.Interfaces;

public interface IReportFileGenerator
{
    string Formato { get; }  // "PDF" | "EXCEL"
    Task<byte[]> GenerateAsync(CashClosingReportData data, int corteId, DateTime fecha);
}
```

**Step 3: IEmailService**

```csharp
// src/LaundryManagement.Application/Interfaces/IEmailService.cs
namespace LaundryManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(
        string[]          recipients,
        string            subject,
        string            body,
        string            fileName,
        byte[]            attachment,
        CancellationToken ct = default);
}
```

**Step 4: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 5: Commit**

```bash
git add src/LaundryManagement.Application/Interfaces/IReportDataService.cs \
        src/LaundryManagement.Application/Interfaces/IReportFileGenerator.cs \
        src/LaundryManagement.Application/Interfaces/IEmailService.cs
git commit -m "feat: add report service interfaces"
```

---

### Task 4: Crear ReportSettings y actualizar appsettings.json

**Files:**
- Create: `src/LaundryManagement.Infrastructure/Configuration/ReportSettings.cs`
- Modify: `src/LaundryManagement.API/appsettings.json`

**Step 1: Crear ReportSettings**

```csharp
// src/LaundryManagement.Infrastructure/Configuration/ReportSettings.cs
namespace LaundryManagement.Infrastructure.Configuration;

public sealed class ReportSettings
{
    public const string SectionName = "ReportSettings";

    public bool   Enabled      { get; set; } = false;
    public string EmailFrom    { get; set; } = string.Empty;
    public string SmtpHost     { get; set; } = string.Empty;
    public int    SmtpPort     { get; set; } = 587;
    public string SmtpUser     { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool   SmtpUseSsl   { get; set; } = false;
}
```

**Step 2: Agregar sección en appsettings.json**

En `src/LaundryManagement.API/appsettings.json`, agregar antes del cierre del JSON raíz:

```json
"ReportSettings": {
  "Enabled": false,
  "EmailFrom": "sistema@lavanderia.com",
  "SmtpHost": "smtp.example.com",
  "SmtpPort": 587,
  "SmtpUser": "",
  "SmtpPassword": "",
  "SmtpUseSsl": false
}
```

**Step 3: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 4: Commit**

```bash
git add src/LaundryManagement.Infrastructure/Configuration/ReportSettings.cs \
        src/LaundryManagement.API/appsettings.json
git commit -m "feat: add ReportSettings configuration"
```

---

### Task 5: Implementar ReportDataService (Dapper)

**Files:**
- Create: `src/LaundryManagement.Infrastructure/Services/ReportDataService.cs`

Los datos del reporte se extraen de 2 consultas:
- **Corte + Cajero** → secciones VentasDia y CorteCaja (usando los totales del propio corte)
- **Balanza** → órdenes con saldo pendiente (Total - Pagado) agrupadas por si son de hoy o anteriores

**Step 1: Crear ReportDataService**

```csharp
// src/LaundryManagement.Infrastructure/Services/ReportDataService.cs
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

            var fondoInicial  = corte.FondoInicial ?? 0m;
            var sistemaEfect  = corte.TotalEsperadoEfectivo;
            var retiroEfect   = corte.TotalDeclaradoEfectivo;
            var ajuste        = corte.MontoAjuste;
            var fondoFinal    = fondoInicial + sistemaEfect - retiroEfect + ajuste;

            return new CashClosingReportData(
                Cashier: new CashierInfo(
                    Nombre: corte.CajeroNombre,
                    Turno:  corte.TurnoDescripcion ?? string.Empty,
                    Fecha:  corte.FechaCorte),
                VentasDia: new VentasDiaSection(
                    Efectivo:       corte.TotalEsperadoEfectivo,
                    Tarjeta:        corte.TotalEsperadoTarjeta,
                    Transferencia:  corte.TotalEsperadoTransferencia,
                    Otros:          corte.TotalEsperadoOtros,
                    TotalVentas:    corte.TotalEsperado),
                Balanza: new BalanzaSection(
                    PendienteHoy:    balanza.PendienteHoy,
                    AcumuladoAyer:   balanza.AcumuladoAyer,
                    NuevoAcumulado:  balanza.PendienteHoy + balanza.AcumuladoAyer),
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
```

> **Nota:** Las tablas en SQL usan nombres en plural sin acento: `CortesCaja`, `Ordenes`, `Pagos`, `Usuarios`. Verificar el nombre exacto de las columnas si el build o la query falla.

**Step 2: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Infrastructure/Services/ReportDataService.cs
git commit -m "feat: implement ReportDataService with Dapper"
```

---

### Task 6: Implementar PdfReportGenerator (QuestPDF)

**Files:**
- Create: `src/LaundryManagement.Infrastructure/Reports/PdfReportGenerator.cs`

**Step 1: Registrar licencia de QuestPDF en Program.cs**

En `src/LaundryManagement.API/Program.cs`, agregar antes de `builder.Build()`:

```csharp
// QuestPDF — Community license (gratis para uso comercial con ingresos < $1M USD/año)
QuestPDF.Infrastructure.QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
```

Y el using al inicio del archivo:
```csharp
using QuestPDF.Infrastructure;
```

**Step 2: Crear PdfReportGenerator**

```csharp
// src/LaundryManagement.Infrastructure/Reports/PdfReportGenerator.cs
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
                        ("TOTAL EFECTIVO",      data.VentasDia.Efectivo,       0m,                         data.VentasDia.Efectivo),
                        ("TOTAL TARJETA",       data.VentasDia.Tarjeta,        0m,                         data.VentasDia.Efectivo + data.VentasDia.Tarjeta),
                        ("TOTAL TRANSFERENCIA", data.VentasDia.Transferencia,  0m,                         data.VentasDia.Efectivo + data.VentasDia.Tarjeta + data.VentasDia.Transferencia),
                        ("TOTAL OTROS",         data.VentasDia.Otros,          0m,                         data.VentasDia.TotalVentas - data.VentasDia.Transferencia - data.VentasDia.Tarjeta - data.VentasDia.Efectivo + data.VentasDia.Otros),
                        ("TOTAL VENTAS DEL DÍA",0m,                            data.VentasDia.TotalVentas, 0m),
                    }));

                    col.Item().PaddingTop(10)
                        .Text("BALANZA \"PENDIENTE DE COBRO\"").Bold().FontSize(11);
                    col.Item().Element(e => BuildTable(e, new[]
                    {
                        ("PENDIENTE POR COBRAR HOY", data.Balanza.PendienteHoy,  0m,                          data.Balanza.PendienteHoy),
                        ("ACUMULADO AL DÍA DE AYER", data.Balanza.AcumuladoAyer, 0m,                          data.Balanza.PendienteHoy + data.Balanza.AcumuladoAyer),
                        ("NUEVO ACUMULADO A HOY",    0m,                          data.Balanza.NuevoAcumulado, 0m),
                    }));

                    col.Item().PaddingTop(10)
                        .Text("CORTE DE CAJA CHICA").Bold().FontSize(11);
                    col.Item().Element(e => BuildTable(e, new[]
                    {
                        ("FONDO INICIAL",          data.CorteCaja.FondoInicial,    0m,                          data.CorteCaja.FondoInicial),
                        ("CORTE SISTEMA EFECTIVO", data.CorteCaja.SistemaEfectivo, 0m,                          data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo),
                        ("RETIRO EFECTIVO",        0m,                              data.CorteCaja.RetiroEfectivo, data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo - data.CorteCaja.RetiroEfectivo),
                        ("AJUSTE DE CAJA",         data.CorteCaja.AjusteCaja,      0m,                          data.CorteCaja.FondoFinal),
                        ("FONDO FINAL",            0m,                              0m,                          data.CorteCaja.FondoFinal),
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

            // Header
            static IContainer HeaderCell(IContainer c) =>
                c.Background(Colors.Grey.Lighten2).Padding(4);

            table.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("CONCEPTO").Bold();
                h.Cell().Element(HeaderCell).AlignRight().Text("CARGO").Bold();
                h.Cell().Element(HeaderCell).AlignRight().Text("ABONO").Bold();
                h.Cell().Element(HeaderCell).AlignRight().Text("SALDO").Bold();
            });

            // Rows
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
```

**Step 3: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 4: Commit**

```bash
git add src/LaundryManagement.Infrastructure/Reports/PdfReportGenerator.cs \
        src/LaundryManagement.API/Program.cs
git commit -m "feat: implement PdfReportGenerator with QuestPDF"
```

---

### Task 7: Implementar ExcelReportGenerator (ClosedXML)

**Files:**
- Create: `src/LaundryManagement.Infrastructure/Reports/ExcelReportGenerator.cs`

**Step 1: Crear ExcelReportGenerator**

```csharp
// src/LaundryManagement.Infrastructure/Reports/ExcelReportGenerator.cs
using ClosedXML.Excel;
using LaundryManagement.Application.DTOs.Reports;
using LaundryManagement.Application.Interfaces;

namespace LaundryManagement.Infrastructure.Reports;

public class ExcelReportGenerator : IReportFileGenerator
{
    public string Formato => "EXCEL";

    public Task<byte[]> GenerateAsync(CashClosingReportData data, int corteId, DateTime fecha)
    {
        using var workbook  = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Corte de Caja");

        var row = 1;

        // Título
        ws.Cell(row, 1).Value = $"CORTE DE CAJA — {data.Cashier.Nombre}";
        ws.Range(row, 1, row, 4).Merge().Style.Font.Bold = true;
        ws.Range(row, 1, row, 4).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;

        ws.Cell(row, 1).Value = $"Turno: {data.Cashier.Turno}  |  Fecha: {data.Cashier.Fecha:dd/MM/yyyy}";
        ws.Range(row, 1, row, 4).Merge();
        row += 2;

        // Sección VENTAS DEL DÍA
        row = WriteSection(ws, row, "VENTAS DEL DÍA", new[]
        {
            ("TOTAL EFECTIVO",      data.VentasDia.Efectivo,       0m,                         data.VentasDia.Efectivo),
            ("TOTAL TARJETA",       data.VentasDia.Tarjeta,        0m,                         data.VentasDia.Efectivo + data.VentasDia.Tarjeta),
            ("TOTAL TRANSFERENCIA", data.VentasDia.Transferencia,  0m,                         data.VentasDia.Efectivo + data.VentasDia.Tarjeta + data.VentasDia.Transferencia),
            ("TOTAL OTROS",         data.VentasDia.Otros,          0m,                         data.VentasDia.TotalVentas),
            ("TOTAL VENTAS DEL DÍA",0m,                            data.VentasDia.TotalVentas, 0m),
        });

        row++;

        // Sección BALANZA
        row = WriteSection(ws, row, "BALANZA \"PENDIENTE DE COBRO\"", new[]
        {
            ("PENDIENTE POR COBRAR HOY", data.Balanza.PendienteHoy,  0m,                          data.Balanza.PendienteHoy),
            ("ACUMULADO AL DÍA DE AYER", data.Balanza.AcumuladoAyer, 0m,                          data.Balanza.NuevoAcumulado),
            ("NUEVO ACUMULADO A HOY",    0m,                          data.Balanza.NuevoAcumulado, 0m),
        });

        row++;

        // Sección CORTE DE CAJA CHICA
        WriteSection(ws, row, "CORTE DE CAJA CHICA", new[]
        {
            ("FONDO INICIAL",          data.CorteCaja.FondoInicial,    0m,                          data.CorteCaja.FondoInicial),
            ("CORTE SISTEMA EFECTIVO", data.CorteCaja.SistemaEfectivo, 0m,                          data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo),
            ("RETIRO EFECTIVO",        0m,                              data.CorteCaja.RetiroEfectivo, data.CorteCaja.FondoInicial + data.CorteCaja.SistemaEfectivo - data.CorteCaja.RetiroEfectivo),
            ("AJUSTE DE CAJA",         data.CorteCaja.AjusteCaja,      0m,                          data.CorteCaja.FondoFinal),
            ("FONDO FINAL",            0m,                              0m,                          data.CorteCaja.FondoFinal),
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
        ws.Range(row, 1, row, 4).Merge().Style.Fill.BackgroundColor = XLColor.LightGray;
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
            ws.Cell(row, 2).Value = cargo > 0 ? (object)cargo : "-";
            ws.Cell(row, 3).Value = abono > 0 ? (object)abono : "-";
            ws.Cell(row, 4).Value = saldo > 0 ? (object)saldo : "-";

            if (cargo > 0) ws.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            if (abono > 0) ws.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
            if (saldo > 0) ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";

            row++;
        }

        return row;
    }
}
```

**Step 2: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Infrastructure/Reports/ExcelReportGenerator.cs
git commit -m "feat: implement ExcelReportGenerator with ClosedXML"
```

---

### Task 8: Implementar SmtpEmailService

**Files:**
- Create: `src/LaundryManagement.Infrastructure/Services/SmtpEmailService.cs`

**Step 1: Crear SmtpEmailService**

```csharp
// src/LaundryManagement.Infrastructure/Services/SmtpEmailService.cs
using System.Net;
using System.Net.Mail;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LaundryManagement.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ReportSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<ReportSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    public async Task SendAsync(
        string[]          recipients,
        string            subject,
        string            body,
        string            fileName,
        byte[]            attachment,
        CancellationToken ct = default)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl   = _settings.SmtpUseSsl,
            Credentials = string.IsNullOrEmpty(_settings.SmtpUser)
                ? null
                : new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword)
        };

        using var message = new MailMessage
        {
            From    = new MailAddress(_settings.EmailFrom),
            Subject = subject,
            Body    = body,
            IsBodyHtml = false
        };

        foreach (var r in recipients)
            message.To.Add(r.Trim());

        using var ms         = new MemoryStream(attachment);
        using var attachObj  = new Attachment(ms, fileName);
        message.Attachments.Add(attachObj);

        _logger.LogInformation("Enviando reporte {FileName} a {Count} destinatarios", fileName, recipients.Length);
        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email enviado exitosamente: {Subject}", subject);
    }
}
```

**Step 2: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Infrastructure/Services/SmtpEmailService.cs
git commit -m "feat: implement SmtpEmailService"
```

---

### Task 9: Crear GenerateReportOnCashClosingCreatedHandler

**Files:**
- Create: `src/LaundryManagement.Application/EventHandlers/CashClosings/GenerateReportOnCashClosingCreatedHandler.cs`

Este handler:
1. Consulta `ConfiguracionReporte` activos con `TipoReporte = "CorteCaja"` via EF Core (inyecta `LaundryDbContext` directamente desde Infrastructure — el handler vive en Application, pero puede hacerlo a través de la interfaz de repositorio; sin embargo, dado que no hay repositorio para `ConfiguracionReporte`, inyectaremos `IReportConfigRepository` — pero eso añade complejidad innecesaria. En cambio, el handler puede depender de una interfaz `IReportConfigService` sencilla).

**Step 1: Crear interfaz IReportConfigService en Application**

```csharp
// src/LaundryManagement.Application/Interfaces/IReportConfigService.cs
namespace LaundryManagement.Application.Interfaces;

public record ReportConfig(
    int     ConfigReporteId,
    string  NombreReporte,
    string  FormatoExportacion,
    string? DestinatariosEmail
);

public interface IReportConfigService
{
    Task<IReadOnlyList<ReportConfig>> GetActiveCashClosingConfigsAsync(CancellationToken ct = default);
    Task SaveHistorialAsync(int configId, string estado, string? mensajeError = null, CancellationToken ct = default);
}
```

**Step 2: Implementar ReportConfigService en Infrastructure**

```csharp
// src/LaundryManagement.Infrastructure/Services/ReportConfigService.cs
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Services;

public class ReportConfigService : IReportConfigService
{
    private readonly LaundryDbContext _context;

    public ReportConfigService(LaundryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ReportConfig>> GetActiveCashClosingConfigsAsync(CancellationToken ct = default)
    {
        return await _context.ConfiguracionReportes
            .Where(c => c.Activo && c.TipoReporte == "CorteCaja")
            .Select(c => new ReportConfig(
                c.ConfigReporteId,
                c.NombreReporte,
                c.FormatoExportacion,
                c.DestinatariosEmail))
            .ToListAsync(ct);
    }

    public async Task SaveHistorialAsync(int configId, string estado, string? mensajeError = null, CancellationToken ct = default)
    {
        _context.HistorialReportes.Add(new HistorialReporte
        {
            ConfigReporteId  = configId,
            FechaGeneracion  = DateTime.Now,
            Estado           = estado,
            MensajeError     = mensajeError,
            RutaArchivo      = null
        });
        await _context.SaveChangesAsync(ct);
    }
}
```

**Step 3: Crear el event handler**

```csharp
// src/LaundryManagement.Application/EventHandlers/CashClosings/GenerateReportOnCashClosingCreatedHandler.cs
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Notifications;
using LaundryManagement.Domain.DomainEvents.CashClosings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.EventHandlers.CashClosings;

/// <summary>
/// Genera reportes configurados en ConfiguracionReporte después de registrar un corte de caja.
/// Fire-and-forget: cualquier fallo se loguea y registra en HistorialReporte, nunca propaga al caller.
/// </summary>
public sealed class GenerateReportOnCashClosingCreatedHandler
    : INotificationHandler<DomainEventNotification<CashClosingCreated>>
{
    private readonly IReportConfigService  _configService;
    private readonly IReportDataService    _dataService;
    private readonly IEnumerable<IReportFileGenerator> _generators;
    private readonly IEmailService         _emailService;
    private readonly ILogger<GenerateReportOnCashClosingCreatedHandler> _logger;

    public GenerateReportOnCashClosingCreatedHandler(
        IReportConfigService  configService,
        IReportDataService    dataService,
        IEnumerable<IReportFileGenerator> generators,
        IEmailService         emailService,
        ILogger<GenerateReportOnCashClosingCreatedHandler> logger)
    {
        _configService = configService;
        _dataService   = dataService;
        _generators    = generators;
        _emailService  = emailService;
        _logger        = logger;
    }

    public async Task Handle(
        DomainEventNotification<CashClosingCreated> notification,
        CancellationToken cancellationToken)
    {
        var ev = notification.DomainEvent;
        _logger.LogInformation(
            "Procesando reportes para CorteID={CorteId}", ev.CashClosingId);

        IReadOnlyList<ReportConfig> configs;
        try
        {
            configs = await _configService.GetActiveCashClosingConfigsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer ConfiguracionReporte para CorteID={CorteId}", ev.CashClosingId);
            return;
        }

        if (configs.Count == 0)
        {
            _logger.LogInformation("Sin configuraciones activas de reporte para CorteCaja.");
            return;
        }

        foreach (var config in configs)
        {
            await ProcessConfigAsync(config, ev, cancellationToken);
        }
    }

    private async Task ProcessConfigAsync(
        ReportConfig config,
        CashClosingCreated ev,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Generando reporte '{Nombre}' ({Formato}) para CorteID={CorteId}",
            config.NombreReporte, config.FormatoExportacion, ev.CashClosingId);

        // 1. Obtener datos
        CashClosingReportData? data;
        try
        {
            data = await _dataService.GetCashClosingReportDataAsync(
                ev.CashClosingId, ev.ClosingDate, ev.ClosingDate, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener datos del reporte '{Nombre}' CorteID={CorteId}",
                config.NombreReporte, ev.CashClosingId);
            await TrySaveHistorialAsync(config.ConfigReporteId, "Error", ex.Message, ct);
            return;
        }

        // 2. Generar archivo
        var generator = _generators.FirstOrDefault(g =>
            g.Formato.Equals(config.FormatoExportacion, StringComparison.OrdinalIgnoreCase));

        if (generator is null)
        {
            var msg = $"No hay generador para formato '{config.FormatoExportacion}'";
            _logger.LogWarning(msg + " — CorteID={CorteId}", ev.CashClosingId);
            await TrySaveHistorialAsync(config.ConfigReporteId, "Error", msg, ct);
            return;
        }

        byte[] fileBytes;
        try
        {
            fileBytes = await generator.GenerateAsync(data, ev.CashClosingId, ev.ClosingDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al generar archivo '{Formato}' para CorteID={CorteId}",
                config.FormatoExportacion, ev.CashClosingId);
            await TrySaveHistorialAsync(config.ConfigReporteId, "Error", ex.Message, ct);
            return;
        }

        // 3. Enviar o registrar como Omitido
        if (string.IsNullOrWhiteSpace(config.DestinatariosEmail))
        {
            _logger.LogInformation(
                "Reporte '{Nombre}' sin destinatarios — registrado como Omitido.", config.NombreReporte);
            await TrySaveHistorialAsync(config.ConfigReporteId, "Omitido", null, ct);
            return;
        }

        var recipients = config.DestinatariosEmail
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var extension = config.FormatoExportacion.Equals("PDF", StringComparison.OrdinalIgnoreCase)
            ? "pdf" : "xlsx";
        var fileName  = $"corte-{ev.CashClosingId}-{ev.ClosingDate:yyyy-MM-dd}.{extension}";
        var subject   = $"Corte de Caja — {data.Cashier.Nombre} — {ev.ClosingDate:dd/MM/yyyy}";
        var body      = $"Adjunto el reporte de corte de caja del día {ev.ClosingDate:dd/MM/yyyy}.";

        try
        {
            await _emailService.SendAsync(recipients, subject, body, fileName, fileBytes, ct);
            await TrySaveHistorialAsync(config.ConfigReporteId, "Enviado", null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar email para reporte '{Nombre}' CorteID={CorteId}",
                config.NombreReporte, ev.CashClosingId);
            await TrySaveHistorialAsync(config.ConfigReporteId, "Error", ex.Message, ct);
        }
    }

    private async Task TrySaveHistorialAsync(int configId, string estado, string? mensajeError, CancellationToken ct)
    {
        try
        {
            await _configService.SaveHistorialAsync(configId, estado, mensajeError, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo guardar HistorialReporte para ConfigID={ConfigId}", configId);
        }
    }
}
```

> **Nota:** El handler importa `CashClosingReportData` directamente. Asegurarse que el namespace `LaundryManagement.Application.DTOs.Reports` esté disponible (el handler vive en Application, que ya puede ver sus propios DTOs).

**Step 4: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 5: Commit**

```bash
git add src/LaundryManagement.Application/Interfaces/IReportConfigService.cs \
        src/LaundryManagement.Infrastructure/Services/ReportConfigService.cs \
        src/LaundryManagement.Application/EventHandlers/CashClosings/GenerateReportOnCashClosingCreatedHandler.cs
git commit -m "feat: implement report event handler for CashClosingCreated"
```

---

### Task 10: Publicar CashClosingCreated en CreateCashClosingCommandHandler

**Files:**
- Modify: `src/LaundryManagement.Application/Commands/CashClosings/CreateCashClosingCommandHandler.cs`

El handler actual NO publica el evento. Hay que inyectar `IPublisher` y publicarlo post-commit, igual que hace `CreateOrderCommandHandler`.

**Step 1: Modificar el handler**

Agregar `IPublisher _publisher` al constructor e inyectarlo. Después del `_repository.AddAsync(cashClosing, ct)`, agregar:

```csharp
// Al final de las dependencias inyectadas:
private readonly IPublisher _publisher;

// Constructor actualizado:
public CreateCashClosingCommandHandler(
    ICashClosingRepository repository,
    IPublisher publisher,
    ILogger<CreateCashClosingCommandHandler> logger)
{
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    _publisher  = publisher  ?? throw new ArgumentNullException(nameof(publisher));
    _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
}
```

Después de la línea `var saved = await _repository.AddAsync(cashClosing, ct);` y antes del `return`, agregar:

```csharp
// Publicar evento post-commit (fire-and-forget)
try
{
    var domainEvent = new CashClosingCreated(
        cashClosingId:   saved.Id.Value,
        cashierId:       cmd.CajeroID,
        closingDate:     cmd.FechaCorte,
        totalExpected:   cmd.TotalEsperado,
        totalDeclared:   cmd.TotalDeclarado,
        finalDifference: cmd.TotalDeclarado - cmd.TotalEsperado);

    await _publisher.Publish(
        new DomainEventNotification<CashClosingCreated>(domainEvent), ct);
}
catch (Exception ex)
{
    _logger.LogError(ex,
        "Error al publicar evento CashClosingCreated para CorteID={CorteId}. El registro NO fue afectado.",
        saved.Id.Value);
}
```

Agregar los usings necesarios:
```csharp
using LaundryManagement.Application.Notifications;
using LaundryManagement.Domain.DomainEvents.CashClosings;
using MediatR;
```

**Step 2: Build**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Application/Commands/CashClosings/CreateCashClosingCommandHandler.cs
git commit -m "feat: publish CashClosingCreated event post-commit"
```

---

### Task 11: Registrar servicios en DependencyInjection

**Files:**
- Modify: `src/LaundryManagement.Infrastructure/DependencyInjection.cs`

**Step 1: Agregar registros**

En el método `AddInfrastructure`, agregar junto al bloque de servicios existentes:

```csharp
// Report Settings
services.Configure<ReportSettings>(configuration.GetSection(ReportSettings.SectionName));

// Report Services
services.AddScoped<IReportDataService,   ReportDataService>();
services.AddScoped<IReportConfigService, ReportConfigService>();
services.AddScoped<IEmailService,        SmtpEmailService>();

// Report File Generators (se resuelven como IEnumerable<IReportFileGenerator>)
services.AddScoped<IReportFileGenerator, PdfReportGenerator>();
services.AddScoped<IReportFileGenerator, ExcelReportGenerator>();
```

Agregar los usings:
```csharp
using LaundryManagement.Infrastructure.Reports;
```

**Step 2: Build final**

```bash
dotnet build LaundryManagement.sln
```
Expected: 0 errors, 0 warnings relevantes.

**Step 3: Commit**

```bash
git add src/LaundryManagement.Infrastructure/DependencyInjection.cs
git commit -m "feat: register report services in DI"
```

---

### Task 12: Prueba manual end-to-end

**Step 1: Insertar un registro en ConfiguracionReporte en BD**

```sql
INSERT INTO ConfiguracionReporte (
    NombreReporte, TipoReporte, Frecuencia, FormatoExportacion,
    DestinatariosEmail, HoraEnvio, Activo, ParametrosJson
) VALUES (
    'Reporte Diario de Corte', 'CorteCaja', 'EventoCorte', 'PDF',
    'tu@email.com', NULL, 1, NULL
);
```

**Step 2: Configurar SMTP real o de prueba en appsettings.json**

Usar Mailtrap, MailHog, o SMTP real. Actualizar `ReportSettings` con valores reales.

**Step 3: Levantar la API**

```bash
cd src/LaundryManagement.API
dotnet run
```

**Step 4: Crear un corte de caja via Swagger**

`POST /api/cortes-caja` con datos de prueba válidos.

**Step 5: Verificar**

- Revisar los logs: debe aparecer "Generando reporte 'Reporte Diario de Corte'..." y "Email enviado exitosamente"
- Revisar la tabla `HistorialReporte` en BD: debe tener un registro con `Estado = 'Enviado'`
- Revisar la bandeja del email destinatario: debe llegar el PDF adjunto
