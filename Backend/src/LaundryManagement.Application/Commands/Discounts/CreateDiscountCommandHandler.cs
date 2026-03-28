using LaundryManagement.Application.DTOs.Discounts;
using LaundryManagement.Domain.Aggregates.Discounts;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Handler para crear un nuevo descuento usando el agregado DiscountPure.
/// </summary>
public sealed class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, DiscountDto>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger<CreateDiscountCommandHandler> _logger;

    public CreateDiscountCommandHandler(
        IDiscountRepository discountRepository,
        ILogger<CreateDiscountCommandHandler> logger)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DiscountDto> Handle(CreateDiscountCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Creating discount: Name={Name}, Type={Type}", cmd.Name, cmd.Type);

        // Validar nombre único
        if (await _discountRepository.ExistsByNameAsync(cmd.Name, ct: ct))
            throw new ConflictException("Ya existe un descuento con ese nombre");

        // Parsear tipo de descuento (frontend values: "None", "Percentage", "FixedAmount")
        var type = ParseDiscountType(cmd.Type);

        // Parsear fechas
        var startDate = !string.IsNullOrWhiteSpace(cmd.StartDate)
            ? DateOnly.Parse(cmd.StartDate)
            : DateOnly.FromDateTime(DateTime.Now);

        var endDate = !string.IsNullOrWhiteSpace(cmd.EndDate)
            ? (DateOnly?)DateOnly.Parse(cmd.EndDate)
            : null;

        // Crear agregado (valida reglas de negocio internamente)
        var discount = DiscountPure.Create(
            name:      cmd.Name,
            type:      type,
            value:     Money.FromDecimal(cmd.Value),
            validFrom: startDate,
            validUntil: endDate
        );

        // Persistir
        var saved = await _discountRepository.AddAsync(discount, ct);
        await _discountRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Discount created: ID={Id}", saved.Id.Value);

        return MapToDto(saved);
    }

    /// <summary>
    /// Traduce el tipo de descuento del frontend al value object de dominio.
    /// Acepta: "None", "Percentage", "FixedAmount" (y sus equivalentes en mayúsculas)
    /// </summary>
    internal static DiscountType ParseDiscountType(string typeString)
    {
        return typeString.Trim().ToUpperInvariant() switch
        {
            "NONE" or "NINGUNO"        => DiscountType.None(),
            "PERCENTAGE" or "PORCENTAJE" => DiscountType.Percentage(),
            "FIXEDAMOUNT" or "FIXED" or "MONTOFIJO" => DiscountType.Fixed(),
            _ => throw new ValidationException(
                $"Tipo de descuento inválido: {typeString}. Use None, Percentage o FixedAmount")
        };
    }

    /// <summary>
    /// Mapea el agregado a DTO con los nombres que espera el frontend.
    /// Type: "None" | "Percentage" | "FixedAmount"
    /// </summary>
    internal static DiscountDto MapToDto(DiscountPure discount) => new()
    {
        Id             = discount.Id.Value,
        Name           = discount.Name,
        Type           = discount.Type.IsNone ? "None"
                       : discount.Type.IsPercentage ? "Percentage"
                       : "FixedAmount",
        Value          = discount.Value.Amount,
        IsActive       = discount.IsActive,
        MinOrderAmount = null,
        StartDate      = discount.ValidFrom.ToString("yyyy-MM-dd"),
        EndDate        = discount.ValidUntil?.ToString("yyyy-MM-dd"),
        CreatedAt      = discount.ValidFrom.ToString("yyyy-MM-dd") + "T00:00:00",
        UpdatedAt      = null
    };
}
