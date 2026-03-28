using LaundryManagement.Application.DTOs.Reportes;

namespace LaundryManagement.Application.Interfaces;

public interface IReporteService
{
    Task<VentaDiariaResponse> ReporteVentasDiariasAsync(DateTime fecha);
}
