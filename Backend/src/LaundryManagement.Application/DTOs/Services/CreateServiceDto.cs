namespace LaundryManagement.Application.DTOs.Services;

/// <summary>
/// DTO para crear un nuevo servicio
/// </summary>
public class CreateServiceDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string UnitType { get; set; } = string.Empty; // "piece" | "kg"
    public decimal BasePrice { get; set; }
    public decimal? PricePerKilo { get; set; } // Para servicios por peso
    public decimal? MinimumWeight { get; set; } // Para servicios por peso
    public decimal? MaximumWeight { get; set; } // Para servicios por peso
    public string? Icon { get; set; }
    public decimal? EstimatedHours { get; set; }
}
