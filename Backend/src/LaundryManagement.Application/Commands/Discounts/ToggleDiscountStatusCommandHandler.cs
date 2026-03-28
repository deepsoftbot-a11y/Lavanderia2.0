using LaundryManagement.Application.DTOs.Discounts;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Handler para alternar el estado activo/inactivo de un descuento.
/// </summary>
public sealed class ToggleDiscountStatusCommandHandler : IRequestHandler<ToggleDiscountStatusCommand, DiscountDto>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger<ToggleDiscountStatusCommandHandler> _logger;

    public ToggleDiscountStatusCommandHandler(
        IDiscountRepository discountRepository,
        ILogger<ToggleDiscountStatusCommandHandler> logger)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DiscountDto> Handle(ToggleDiscountStatusCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Toggling discount status: ID={Id}", cmd.DiscountId);

        var discount = await _discountRepository.GetByIdAsync(DiscountId.From(cmd.DiscountId), ct)
            ?? throw new NotFoundException($"Descuento con ID {cmd.DiscountId} no encontrado");

        // Alternar estado via métodos de dominio
        // Deactivate() lanza ConflictException internamente si es tipo NONE
        if (discount.IsActive)
            discount.Deactivate();
        else
            discount.Activate();

        await _discountRepository.UpdateAsync(discount, ct);
        await _discountRepository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Discount status toggled: ID={Id}, IsActive={IsActive}",
            discount.Id.Value,
            discount.IsActive
        );

        return CreateDiscountCommandHandler.MapToDto(discount);
    }
}
