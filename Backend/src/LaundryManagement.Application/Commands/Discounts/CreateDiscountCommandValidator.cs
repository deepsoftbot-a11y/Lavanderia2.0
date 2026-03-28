using FluentValidation;

namespace LaundryManagement.Application.Commands.Discounts;

/// <summary>
/// Validador para CreateDiscountCommand usando FluentValidation
/// </summary>
public sealed class CreateDiscountCommandValidator : AbstractValidator<CreateDiscountCommand>
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "None", "Percentage", "FixedAmount"
    };

    public CreateDiscountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del descuento es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de descuento es requerido")
            .Must(t => ValidTypes.Contains(t))
            .WithMessage("El tipo debe ser None, Percentage o FixedAmount");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0m).WithMessage("El valor no puede ser negativo");

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
