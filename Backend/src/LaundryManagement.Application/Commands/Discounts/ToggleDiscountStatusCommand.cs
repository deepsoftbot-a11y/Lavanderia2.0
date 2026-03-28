using LaundryManagement.Application.DTOs.Discounts;
using MediatR;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Comando para alternar el estado activo/inactivo de un descuento.
/// El descuento de tipo NONE no puede ser desactivado (se valida en el agregado).
/// </summary>
public sealed record ToggleDiscountStatusCommand : IRequest<DiscountDto>
{
    public int DiscountId { get; init; }

    public ToggleDiscountStatusCommand(int discountId)
    {
        DiscountId = discountId;
    }
}
