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
