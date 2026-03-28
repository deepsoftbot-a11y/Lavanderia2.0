using LaundryManagement.Application.DTOs.Reports;
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
    private readonly IReportConfigService _configService;
    private readonly IReportDataService   _dataService;
    private readonly IEnumerable<IReportFileGenerator> _generators;
    private readonly IEmailService        _emailService;
    private readonly ILogger<GenerateReportOnCashClosingCreatedHandler> _logger;

    public GenerateReportOnCashClosingCreatedHandler(
        IReportConfigService configService,
        IReportDataService   dataService,
        IEnumerable<IReportFileGenerator> generators,
        IEmailService        emailService,
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
        _logger.LogInformation("Procesando reportes para CorteID={CorteId}", ev.CashClosingId);

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
