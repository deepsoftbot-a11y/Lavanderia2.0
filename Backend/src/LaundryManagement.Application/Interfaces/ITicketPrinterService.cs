using LaundryManagement.Application.Queries.Orders;

namespace LaundryManagement.Application.Interfaces;

public interface ITicketPrinterService
{
    /// <summary>
    /// Imprime el ticket de recepción para la orden dada.
    /// Las implementaciones deben capturar sus propias excepciones (best-effort).
    /// </summary>
    Task PrintOrderTicketAsync(OrderResponseDto order, CancellationToken ct = default);
}
