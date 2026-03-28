using MediatR;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Comando para eliminar un descuento del catálogo (hard delete).
/// El descuento de tipo NONE no puede ser eliminado.
/// </summary>
public sealed record DeleteDiscountCommand : IRequest<Unit>
{
    public int DiscountId { get; init; }
}
