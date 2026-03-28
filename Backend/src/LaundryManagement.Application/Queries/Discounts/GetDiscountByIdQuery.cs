using LaundryManagement.Application.DTOs.Discounts;
using MediatR;

namespace LaundryManagement.Application.Queries.Discounts;

/// <summary>
/// Query para obtener un descuento por su ID
/// </summary>
public sealed record GetDiscountByIdQuery : IRequest<DiscountDto?>
{
    public int DiscountId { get; init; }

    public GetDiscountByIdQuery(int discountId)
    {
        DiscountId = discountId;
    }
}
