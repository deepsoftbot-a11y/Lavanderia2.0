using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.DTOs.Pagos;
using LaundryManagement.Application.DTOs.Payments;

namespace LaundryManagement.Application.Interfaces;

public interface IPagoService
{
    Task<RegistrarPagoResponse> RegistrarPagoAsync(RegistrarPagoRequest request);
    Task<ConsultarSaldoClienteResponse> ConsultarSaldoClienteAsync(int clienteID);

    /// <summary>Retorna el total pagado para una orden específica (excluye cancelados).</summary>
    Task<decimal> GetAmountPaidByOrderAsync(int orderId);

    /// <summary>Retorna el total pagado agrupado por orden (excluye cancelados). Clave = OrdenId.</summary>
    Task<Dictionary<int, decimal>> GetAmountsPaidByOrdersAsync(IEnumerable<int> orderIds);

    /// <summary>Retorna la lista de pagos de cada orden (excluye cancelados). Clave = OrdenId.</summary>
    Task<Dictionary<int, List<OrderPaymentDto>>> GetPaymentsByOrdersAsync(IEnumerable<int> orderIds);

    /// <summary>Crea un pago a partir del input del frontend y retorna el pago creado.</summary>
    Task<OrderPaymentDto> CreatePaymentAsync(CreatePaymentRequest request);

    /// <summary>Retorna un pago por su ID, o null si no existe o está cancelado.</summary>
    Task<OrderPaymentDto?> GetPaymentByIdAsync(int paymentId);

    /// <summary>Retorna los pagos activos de una orden.</summary>
    Task<List<OrderPaymentDto>> GetPaymentsByOrderIdAsync(int orderId);

    /// <summary>Cancela (borrado lógico) un pago existente.</summary>
    Task CancelPaymentAsync(int paymentId, int cancelledBy);
}
