using LaundryManagement.Application.DTOs;

namespace LaundryManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default);
}
