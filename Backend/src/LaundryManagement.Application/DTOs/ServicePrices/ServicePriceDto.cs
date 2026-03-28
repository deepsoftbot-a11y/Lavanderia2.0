using LaundryManagement.Application.DTOs.ServiceGarments;

namespace LaundryManagement.Application.DTOs.ServicePrices;

/// <summary>
/// DTO que representa un precio de servicio-prenda para el frontend.
/// Coincide con el tipo TypeScript ServiceGarment del plan.
/// </summary>
public class ServicePriceDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public int GarmentTypeId { get; set; }
    public ServiceGarmentDto? GarmentType { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty; // ISO string
    public string? UpdatedAt { get; set; } // ISO string
}
