namespace LaundryManagement.Application.DTOs.Cortes;

public class ConsultarHistorialCortesRequest
{
    public int? CajeroID { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool SoloConDiferencias { get; set; }
}

public class HistorialCorteResponse
{
    public int CorteID { get; set; }
    public int CajeroID { get; set; }
    public string? NombreCajero { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? TurnoDescripcion { get; set; }
    public decimal TotalDeclarado { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal Diferencia { get; set; }
}
