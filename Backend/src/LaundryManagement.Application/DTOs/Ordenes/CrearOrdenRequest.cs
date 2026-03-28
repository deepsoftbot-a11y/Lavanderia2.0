namespace LaundryManagement.Application.DTOs.Ordenes;

public class CrearOrdenRequest
{
    public int ClienteID { get; set; }
    public DateTime FechaPrometida { get; set; }
    public int RecibidoPor { get; set; }
    public string DetalleJSON { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Ubicaciones { get; set; }
}

public class CrearOrdenResponse
{
    public int OrdenID { get; set; }
}
