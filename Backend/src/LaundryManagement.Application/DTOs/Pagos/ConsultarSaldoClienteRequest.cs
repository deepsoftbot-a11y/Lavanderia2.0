namespace LaundryManagement.Application.DTOs.Pagos;

public class ConsultarSaldoClienteRequest
{
    public int ClienteID { get; set; }
}

public class ConsultarSaldoClienteResponse
{
    public int ClienteID { get; set; }
    public string? NombreCliente { get; set; }
    public decimal TotalOrdenes { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}
