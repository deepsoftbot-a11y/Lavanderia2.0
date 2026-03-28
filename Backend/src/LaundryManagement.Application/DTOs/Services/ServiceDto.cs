namespace LaundryManagement.Application.DTOs.Services;

/// <summary>
/// DTO que representa un servicio para el frontend.
/// Coincide con el tipo TypeScript Service del plan.
/// </summary>
public class ServiceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto Category { get; set; } = null!;
    public string ChargeType { get; set; } = string.Empty; // "piece" | "kg"
    public decimal PricePerKg { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public bool IsActive { get; set; }
    public string? Icon { get; set; }
    public decimal? EstimatedTime { get; set; }
    public string CreatedAt { get; set; } = string.Empty; // ISO string
    public string? UpdatedAt { get; set; } // ISO string
}

/// <summary>
/// DTO simplificado para la categoría dentro de ServiceDto
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
