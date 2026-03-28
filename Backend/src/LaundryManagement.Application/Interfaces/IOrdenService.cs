using LaundryManagement.Application.DTOs.Ordenes;

namespace LaundryManagement.Application.Interfaces;

public interface IOrdenService
{
    Task<CrearOrdenResponse> CrearOrdenAsync(CrearOrdenRequest request);
    Task CambiarEstadoOrdenAsync(CambiarEstadoOrdenRequest request);
}
