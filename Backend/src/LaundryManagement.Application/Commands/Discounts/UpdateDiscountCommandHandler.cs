using LaundryManagement.Application.DTOs.Discounts;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Handler para actualizar un descuento existente usando el agregado DiscountPure.
/// </summary>
public sealed class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, DiscountDto>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger<UpdateDiscountCommandHandler> _logger;

    public UpdateDiscountCommandHandler(
        IDiscountRepository discountRepository,
        ILogger<UpdateDiscountCommandHandler> logger)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DiscountDto> Handle(UpdateDiscountCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Updating discount: ID={Id}", cmd.DiscountId);

        // Cargar agregado
        var discount = await _discountRepository.GetByIdAsync(DiscountId.From(cmd.DiscountId), ct)
            ?? throw new NotFoundException($"Descuento con ID {cmd.DiscountId} no encontrado");

        // Validar nombre único si cambió
        if (cmd.Name != null && cmd.Name != discount.Name)
        {
            if (await _discountRepository.ExistsByNameAsync(cmd.Name, excludeId: discount.Id, ct: ct))
                throw new ConflictException("Ya existe un descuento con ese nombre");
        }

        // Calcular valores efectivos (null en command = mantener actual)
        var name  = cmd.Name  ?? discount.Name;
        var type  = cmd.Type  != null ? CreateDiscountCommandHandler.ParseDiscountType(cmd.Type) : discount.Type;
        var value = cmd.Value.HasValue ? Money.FromDecimal(cmd.Value.Value) : discount.Value;

        var validFrom = !string.IsNullOrWhiteSpace(cmd.StartDate)
            ? DateOnly.Parse(cmd.StartDate)
            : discount.ValidFrom;

        // EndDate: null = mantener actual; "" = limpiar; fecha válida = actualizar
        DateOnly? validUntil = cmd.EndDate == null
            ? discount.ValidUntil
            : (string.IsNullOrWhiteSpace(cmd.EndDate) ? null : DateOnly.Parse(cmd.EndDate));

        // Actualizar via método de dominio (valida consistencia tipo/valor y fechas)
        discount.UpdateInfo(name, type, value, validFrom, validUntil);

        // Persistir
        await _discountRepository.UpdateAsync(discount, ct);
        await _discountRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Discount updated: ID={Id}", discount.Id.Value);

        return CreateDiscountCommandHandler.MapToDto(discount);
    }
}
