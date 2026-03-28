namespace LaundryManagement.Application.DTOs.Ordenes;

public class CambiarEstadoOrdenRequest
{
    public int OrdenID { get; set; }
    public int NuevoEstadoID { get; set; }
    public int CambiadoPor { get; set; }
    public string? Comentarios { get; set; }
}
