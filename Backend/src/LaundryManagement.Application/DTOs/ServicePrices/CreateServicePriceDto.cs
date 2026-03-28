namespace LaundryManagement.Application.DTOs.ServicePrices;

/// <summary>
/// DTO para crear un nuevo precio de servicio-prenda
/// </summary>
public class CreateServicePriceDto
{
    public int ServiceId { get; set; }
    public int ServiceGarmentId { get; set; }
    public decimal Price { get; set; }
}
