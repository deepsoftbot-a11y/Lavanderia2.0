namespace LaundryManagement.Application.DTOs.ServiceGarments;

/// <summary>
/// DTO para crear un nuevo tipo de prenda
/// </summary>
public class CreateServiceGarmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
