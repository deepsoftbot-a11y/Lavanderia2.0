using FluentValidation;

namespace LaundryManagement.Application.Commands.Services;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code)
                .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres");
        });

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del servicio es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Debe especificar una categoría válida");

        RuleFor(x => x.ChargeType)
            .NotEmpty().WithMessage("El tipo de cobro es requerido")
            .Must(ct => ct.ToLowerInvariant() == "piece" || ct.ToLowerInvariant() == "kg")
            .WithMessage("El tipo de cobro debe ser 'piece' o 'kg'");

        // Precio requerido solo para servicios por peso
        When(x => x.ChargeType.ToLowerInvariant() == "kg", () =>
        {
            RuleFor(x => x.PricePerKg)
                .NotNull().WithMessage("El precio por kilo es requerido")
                .GreaterThan(0).WithMessage("El precio debe ser mayor que cero");

            When(x => x.MinWeight.HasValue, () =>
            {
                RuleFor(x => x.MinWeight)
                    .GreaterThanOrEqualTo(0).WithMessage("El peso mínimo no puede ser negativo");
            });

            When(x => x.MaxWeight.HasValue, () =>
            {
                RuleFor(x => x.MaxWeight)
                    .GreaterThanOrEqualTo(0).WithMessage("El peso máximo no puede ser negativo");
            });

            When(x => x.MinWeight.HasValue && x.MaxWeight.HasValue, () =>
            {
                RuleFor(x => x.MaxWeight)
                    .GreaterThanOrEqualTo(x => x.MinWeight!.Value)
                    .WithMessage("El peso máximo debe ser mayor o igual al peso mínimo");
            });
        });

        When(x => x.EstimatedTime.HasValue, () =>
        {
            RuleFor(x => x.EstimatedTime)
                .GreaterThanOrEqualTo(0).WithMessage("Las horas estimadas no pueden ser negativas");
        });

        When(x => x.GarmentPrices != null && x.GarmentPrices.Count > 0, () =>
        {
            RuleForEach(x => x.GarmentPrices)
                .ChildRules(gp =>
                {
                    gp.RuleFor(x => x.GarmentTypeId)
                        .GreaterThan(0).WithMessage("El ID del tipo de prenda debe ser válido");
                    gp.RuleFor(x => x.UnitPrice)
                        .GreaterThan(0).WithMessage("El precio unitario debe ser mayor que cero");
                });
        });
    }
}
