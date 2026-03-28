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
            ConfigReporteId = configId,
            FechaGeneracion = DateTime.Now,
            Estado          = estado,
            MensajeError    = mensajeError,
            RutaArchivo     = null
        });
        await _context.SaveChangesAsync(ct);
    }
}
