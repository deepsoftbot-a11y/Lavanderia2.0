using FluentValidation;

namespace LaundryManagement.Application.Commands.Services;

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("El ID del servicio debe ser válido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del servicio es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        When(x => x.PricePerKg.HasValue, () =>
        {
            RuleFor(x => x.PricePerKg)
                .GreaterThan(0).WithMessage("El precio debe ser mayor que cero");
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
