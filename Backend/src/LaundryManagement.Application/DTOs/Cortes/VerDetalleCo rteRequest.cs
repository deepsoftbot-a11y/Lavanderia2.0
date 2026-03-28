namespace LaundryManagement.Application.DTOs.Cortes;

public class VerDetalleCorteRequest
{
    public int CorteID { get; set; }
}

public class DetalleCorteResponse
{
    public int CorteID { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? TurnoDescripcion { get; set; }
    public decimal TotalDeclaradoEfectivo { get; set; }
    public decimal TotalDeclaradoTarjeta { get; set; }
    public decimal TotalDeclaradoTransferencia { get; set; }
    public decimal TotalDeclaradoOtros { get; set; }
    public decimal TotalSistemaEfectivo { get; set; }
    public decimal TotalSistemaTarjeta { get; set; }
    public decimal TotalSistemaTransferencia { get; set; }
    public decimal TotalSistemaOtros { get; set; }
    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaTarjeta { get; set; }
    public decimal DiferenciaTransferencia { get; set; }
    public decimal DiferenciaOtros { get; set; }
}
