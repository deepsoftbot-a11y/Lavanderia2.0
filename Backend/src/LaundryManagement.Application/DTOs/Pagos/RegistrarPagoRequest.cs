namespace LaundryManagement.Application.DTOs.Pagos;

public class RegistrarPagoRequest
{
    public int OrdenID { get; set; }
    public decimal MontoPago { get; set; }
    public int RecibioPor { get; set; }
    public string MetodosPagoJSON { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class RegistrarPagoResponse
{
    public int PagoID { get; set; }
}
