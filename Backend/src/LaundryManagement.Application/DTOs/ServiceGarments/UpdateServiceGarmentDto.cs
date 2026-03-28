namespace LaundryManagement.Application.DTOs.ServiceGarments;

/// <summary>
/// DTO para actualizar un tipo de prenda existente
/// </summary>
public class UpdateServiceGarmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
