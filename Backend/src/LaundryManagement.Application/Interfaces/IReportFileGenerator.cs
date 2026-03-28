using LaundryManagement.Application.DTOs.Reports;

namespace LaundryManagement.Application.Interfaces;

public interface IReportFileGenerator
{
    string Formato { get; }  // "PDF" | "EXCEL"
    Task<byte[]> GenerateAsync(CashClosingReportData data, int corteId, DateTime fecha);
}
