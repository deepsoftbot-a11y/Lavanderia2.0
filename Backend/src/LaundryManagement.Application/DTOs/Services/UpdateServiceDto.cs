namespace LaundryManagement.Application.DTOs.Services;

/// <summary>
/// DTO para actualizar un servicio existente
/// </summary>
public class UpdateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; } // Para servicios por pieza
    public decimal? PricePerKilo { get; set; } // Para servicios por peso
    public decimal? MinimumWeight { get; set; } // Para servicios por peso
    public decimal? MaximumWeight { get; set; } // Para servicios por peso
    public string? Icon { get; set; }
    public decimal? EstimatedHours { get; set; }
}
