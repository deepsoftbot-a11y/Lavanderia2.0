using FluentValidation;

namespace LaundryManagement.Application.Commands.Orders;

public sealed class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID de orden inválido");

        When(x => x.PromisedDate.HasValue, () =>
            RuleFor(x => x.PromisedDate!.Value)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("La fecha prometida no puede ser en el pasado"));

        When(x => x.Items != null && x.Items.Count > 0, () =>
        {
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ServiceId).GreaterThan(0).WithMessage("ServiceId inválido");
                item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0");
                item.RuleFor(i => i).Must(i => i.WeightKilos > 0 || i.Quantity > 0)
                    .WithMessage("Cada item debe tener peso o cantidad mayor a 0");
            });
        });
    }
}
