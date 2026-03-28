namespace LaundryManagement.Application.DTOs.Cortes;

public class RegistrarCorteCajaRequest
{
    public int CajeroID { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string TurnoDescripcion { get; set; } = string.Empty;
    public decimal TotalDeclaradoEfectivo { get; set; }
    public decimal TotalDeclaradoTarjeta { get; set; }
    public decimal TotalDeclaradoTransferencia { get; set; }
    public decimal TotalDeclaradoOtros { get; set; }
    public string? Observaciones { get; set; }
}

public class RegistrarCorteCajaResponse
{
    public int CorteID { get; set; }
}
