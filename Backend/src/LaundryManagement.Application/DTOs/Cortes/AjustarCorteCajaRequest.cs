namespace LaundryManagement.Application.DTOs.Cortes;

public class AjustarCorteCajaRequest
{
    public int CorteID { get; set; }
    public decimal MontoAjuste { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
