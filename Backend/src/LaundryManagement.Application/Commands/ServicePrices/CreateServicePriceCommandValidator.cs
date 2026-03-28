using FluentValidation;

namespace LaundryManagement.Application.Commands.ServicePrices;

public sealed class CreateServicePriceCommandValidator : AbstractValidator<CreateServicePriceCommand>
{
    public CreateServicePriceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("El ID del servicio debe ser válido");

        RuleFor(x => x.GarmentTypeId)
            .GreaterThan(0).WithMessage("El ID del tipo de prenda debe ser válido");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor que cero");
    }
}
