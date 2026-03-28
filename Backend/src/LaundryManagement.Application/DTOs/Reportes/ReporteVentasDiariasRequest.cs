namespace LaundryManagement.Application.DTOs.Reportes;

public class ReporteVentasDiariasRequest
{
    public DateTime Fecha { get; set; }
}

public class VentaDiariaResponse
{
    public DateTime Fecha { get; set; }
    public int TotalOrdenes { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalTarjeta { get; set; }
    public decimal TotalTransferencia { get; set; }
    public decimal TotalOtros { get; set; }
}
