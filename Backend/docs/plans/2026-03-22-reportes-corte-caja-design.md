# Diseño: Reportes automáticos post-corte de caja

**Fecha:** 2026-03-22
**Estado:** Aprobado

## Contexto

El sistema tiene una entidad `ConfiguracionReporte` en BD que permite definir reportes configurables.
El objetivo es que, al registrar un corte de caja, se generen automáticamente los reportes activos
de tipo `"CorteCaja"` en PDF y/o Excel, y se envíen por email a los destinatarios configurados.

## Arquitectura y flujo

```
POST /api/cortes-caja
    └─► CreateCashClosingCommandHandler
            ├─► CashClosingPure.Create(...)
            ├─► _repository.AddAsync(cashClosing)       ← commit en BD
            ├─► retorna CreateCashClosingResponse        ← respuesta inmediata al cajero
            └─► _publisher.Publish(CashClosingCreated)  [fire-and-forget]
                    └─► GenerateReportOnCashClosingCreatedHandler
                            ├─► Lee ConfiguracionReporte WHERE Activo=true AND TipoReporte="CorteCaja"
                            ├─► Por cada config activa:
                            │       ├─► IReportDataService.GetCashClosingReportDataAsync(corteId, fechaInicio, fechaFin)
                            │       ├─► IReportFileGenerator.GenerateAsync(data) → byte[]  (en memoria)
                            │       ├─► Si DestinatariosEmail tiene valor:
                            │       │       └─► IEmailService.SendAsync(recipients, subject, body, attachment)
                            │       │               → HistorialReporte(Estado="Enviado")
                            │       │       Si falla email:
                            │       │               → HistorialReporte(Estado="Error", MensajeError)
                            │       └─► Si DestinatariosEmail vacío:
                            │               → HistorialReporte(Estado="Omitido")
                            └─► Cualquier excepcion: log + nunca propaga al caller
```

### Capas involucradas

| Capa | Cambios |
|---|---|
| Domain | Ninguno — `CashClosingCreated` ya existe |
| Application | Nuevo handler + interfaces `IReportDataService`, `IReportFileGenerator`, `IEmailService` |
| Infrastructure | Implementaciones: `ReportDataService` (Dapper), `PdfReportGenerator` (QuestPDF), `ExcelReportGenerator` (ClosedXML), `SmtpEmailService` (System.Net.Mail), acceso a `ConfiguracionReporte`/`HistorialReporte` via EF Core |

## Datos del reporte

`IReportDataService.GetCashClosingReportDataAsync(corteId, fechaInicio, fechaFin)` retorna:

```csharp
public record CashClosingReportData(
    CashierInfo      Cashier,
    VentasDiaSection VentasDia,
    BalanzaSection   Balanza,
    CorteCajaSection CorteCaja
);

public record CashierInfo(string Nombre, string Turno, DateTime Fecha);

public record VentasDiaSection(
    decimal Efectivo,
    decimal Tarjeta,
    decimal Transferencia,
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

### Fuentes de datos (Dapper)

| Seccion | Fuente |
|---|---|
| VentasDia | Pagos + MetodosPago filtrado por FechaInicio/FechaFin del corte |
| Balanza | Ordenes con saldo pendiente (Total - Pagado), acumulado hasta ayer vs hoy |
| CorteCaja | Campos del corte registrado (CorteID): FondoInicial, SistemaEfectivo, RetiroEfectivo, MontoAjuste |

## Generacion de archivos

```csharp
public interface IReportFileGenerator
{
    string Formato { get; }  // "PDF" | "EXCEL"
    Task<byte[]> GenerateAsync(CashClosingReportData data, int corteId, DateTime fecha);
}
```

- `PdfReportGenerator` — QuestPDF (licencia libre para uso comercial)
- `ExcelReportGenerator` — ClosedXML (MIT)

Los archivos se generan **en memoria** (`byte[]`) y no se escriben en disco.

## Email

```csharp
public interface IEmailService
{
    Task SendAsync(string[] recipients, string subject, string body,
                   string fileName, byte[] attachment, CancellationToken ct);
}
```

- Implementacion: `SmtpEmailService` usando `System.Net.Mail` (built-in .NET)
- `DestinatariosEmail` se parsea como lista separada por comas
- Configuracion en `appsettings.json`:

```json
"Reports": {
  "EmailFrom": "sistema@lavanderia.com",
  "SmtpHost": "smtp.ejemplo.com",
  "SmtpPort": 587,
  "SmtpUser": "",
  "SmtpPassword": ""
}
```

## HistorialReporte — estados

| Estado | Condicion |
|---|---|
| `"Enviado"` | Archivo generado y email enviado correctamente |
| `"Omitido"` | `DestinatariosEmail` vacio — no se envia |
| `"Error"` | Cualquier fallo en generacion o envio |

`RutaArchivo` siempre `null` (no se guarda en disco).

## Manejo de errores

Cada `ConfiguracionReporte` activa se procesa de forma independiente. Un fallo en una
no detiene las demas. Ningun error propaga al caller — el corte de caja ya fue confirmado.

```
1. Error al obtener datos   → log + HistorialReporte(Error) → skip esta config
2. Error al generar archivo → log + HistorialReporte(Error) → skip esta config
3. Error al enviar email    → log + HistorialReporte(Error)
```

## Paquetes NuGet a agregar (Infrastructure)

- `QuestPDF` — generacion de PDF
- `ClosedXML` — generacion de Excel

## Archivos nuevos

### Application
- `Application/Interfaces/IReportDataService.cs`
- `Application/Interfaces/IReportFileGenerator.cs`
- `Application/Interfaces/IEmailService.cs`
- `Application/DTOs/Reports/CashClosingReportData.cs`
- `Application/EventHandlers/CashClosings/GenerateReportOnCashClosingCreatedHandler.cs`

### Infrastructure
- `Infrastructure/Services/ReportDataService.cs`
- `Infrastructure/Reports/PdfReportGenerator.cs`
- `Infrastructure/Reports/ExcelReportGenerator.cs`
- `Infrastructure/Services/SmtpEmailService.cs`

### Modificaciones
- `Application/Commands/CashClosings/CreateCashClosingCommandHandler.cs` — publicar `CashClosingCreated` post-commit
- `Infrastructure/DependencyInjection.cs` — registrar nuevos servicios
- `appsettings.json` — agregar seccion `Reports`
