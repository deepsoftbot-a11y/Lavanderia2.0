namespace LaundryManagement.Application.DTOs.ServiceGarments;

/// <summary>
/// DTO que representa un tipo de prenda para el frontend.
/// Coincide con el tipo TypeScript ServiceGarment del plan.
/// </summary>
public class ServiceGarmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty; // ISO string
    public string? UpdatedAt { get; set; } // ISO string
}
