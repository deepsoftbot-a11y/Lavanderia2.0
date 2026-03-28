using FluentValidation;

namespace LaundryManagement.Application.Commands.Services;

public sealed class UpdateServicePriceCommandValidator : AbstractValidator<UpdateServicePriceCommand>
{
    public UpdateServicePriceCommandValidator()
    {
        RuleFor(x => x.ServicePriceId)
            .GreaterThan(0).WithMessage("El ID del precio debe ser válido");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio debe ser mayor que cero");
    }
}
