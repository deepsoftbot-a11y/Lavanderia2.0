using LaundryManagement.Application.DTOs.Reportes;

namespace LaundryManagement.Application.Interfaces;

public interface IReporteService
{
    Task<VentaDiariaResponse> ReporteVentasDiariasAsync(DateTime fecha);

    Task<byte[]> ExportOrdenesExcelAsync(
        DateTime? startDate,
        DateTime? endDate,
        int[]? statusIds,
        string[]? paymentStatuses);

    Task<byte[]> ExportOrdenesPdfAsync(
        DateTime? startDate,
        DateTime? endDate,
        int[]? statusIds,
        string[]? paymentStatuses);
}
