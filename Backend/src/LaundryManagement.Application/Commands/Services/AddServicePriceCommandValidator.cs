using FluentValidation;

namespace LaundryManagement.Application.Commands.Services;

public sealed class AddServicePriceCommandValidator : AbstractValidator<AddServicePriceCommand>
{
    public AddServicePriceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("El ID del servicio debe ser válido");

        RuleFor(x => x.ServiceGarmentId)
            .GreaterThan(0).WithMessage("El ID del tipo de prenda debe ser válido");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor que cero");
    }
}
