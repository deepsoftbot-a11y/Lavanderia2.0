using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Handler para eliminar un descuento.
/// Verifica que no sea el descuento especial "Sin descuento" (tipo NONE).
/// </summary>
public sealed class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, Unit>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger<DeleteDiscountCommandHandler> _logger;

    public DeleteDiscountCommandHandler(
        IDiscountRepository discountRepository,
        ILogger<DeleteDiscountCommandHandler> logger)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeleteDiscountCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Deleting discount: ID={Id}", cmd.DiscountId);

        var discount = await _discountRepository.GetByIdAsync(DiscountId.From(cmd.DiscountId), ct)
            ?? throw new NotFoundException($"Descuento con ID {cmd.DiscountId} no encontrado");

        if (discount.Type.IsNone)
            throw new ConflictException("No se puede eliminar el descuento 'Sin descuento'");

        await _discountRepository.DeleteAsync(discount.Id, ct);
        await _discountRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Discount deleted: ID={Id}", cmd.DiscountId);

        return Unit.Value;
    }
}
