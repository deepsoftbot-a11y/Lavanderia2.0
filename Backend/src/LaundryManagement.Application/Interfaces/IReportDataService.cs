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
