using LaundryManagement.Application.DTOs.Cortes;

namespace LaundryManagement.Application.Interfaces;

public interface ICorteCajaService
{
    Task<RegistrarCorteCajaResponse> RegistrarCorteCajaAsync(RegistrarCorteCajaRequest request);
    Task AjustarCorteCajaAsync(AjustarCorteCajaRequest request);
    Task<IEnumerable<HistorialCorteResponse>> ConsultarHistorialCortesAsync(ConsultarHistorialCortesRequest request);
    Task<DetalleCorteResponse> VerDetalleCorteAsync(int corteID);
}
