using FluentValidation;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Validador para UpdateDiscountCommand usando FluentValidation
/// </summary>
public sealed class UpdateDiscountCommandValidator : AbstractValidator<UpdateDiscountCommand>
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "None", "Percentage", "FixedAmount"
    };

    public UpdateDiscountCommandValidator()
    {
        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("El ID del descuento debe ser válido");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Type), () =>
        {
            RuleFor(x => x.Type)
                .Must(t => t != null && ValidTypes.Contains(t))
                .WithMessage("El tipo debe ser None, Percentage o FixedAmount");
        });

        When(x => x.Value.HasValue, () =>
        {
            RuleFor(x => x.Value!.Value)
                .GreaterThanOrEqualTo(0m).WithMessage("El valor no puede ser negativo");
        });

        When(x => !string.IsNullOrWhiteSpace(x.StartDate), () =>
        {
            RuleFor(x => x.StartDate)
                .Must(v => v != null && DateOnly.TryParse(v, out _))
                .WithMessage("El formato de fecha de inicio no es válido");
        });

        When(x => !string.IsNullOrWhiteSpace(x.EndDate), () =>
        {
            RuleFor(x => x.EndDate)
                .Must(v => v != null && DateOnly.TryParse(v, out _))
                .WithMessage("El formato de fecha de fin no es válido");
        });
    }
}
