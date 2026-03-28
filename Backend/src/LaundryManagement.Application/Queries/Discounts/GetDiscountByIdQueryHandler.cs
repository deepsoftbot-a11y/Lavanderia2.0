using LaundryManagement.Application.Commands.Discounts;
using LaundryManagement.Application.DTOs.Discounts;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Discounts;

/// <summary>
/// Handler para obtener un descuento por su ID.
/// Retorna null si no existe (el controller maneja el 404).
/// </summary>
public sealed class GetDiscountByIdQueryHandler : IRequestHandler<GetDiscountByIdQuery, DiscountDto?>
{
    private readonly IDiscountRepository _discountRepository;

    public GetDiscountByIdQueryHandler(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
    }

    public async Task<DiscountDto?> Handle(GetDiscountByIdQuery query, CancellationToken ct)
    {
        var discount = await _discountRepository.GetByIdAsync(DiscountId.From(query.DiscountId), ct);

        if (discount == null)
            return null;

        return CreateDiscountCommandHandler.MapToDto(discount);
    }
}
