using FluentValidation;

namespace LaundryManagement.Application.Commands.Orders;

/// <summary>
/// Validador para el comando CreateOrder
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage("ClientId must be a valid client");

        RuleFor(x => x.PromisedDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("PromisedDate cannot be in the past");

        RuleFor(x => x.ReceivedBy)
            .GreaterThan(0)
            .WithMessage("ReceivedBy must be a valid user");

        RuleFor(x => x.InitialStatusId)
            .GreaterThan(0)
            .WithMessage("InitialStatusId must be valid");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("ServiceId must be valid");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0)
                .WithMessage("UnitPrice must be greater than zero");

            item.RuleFor(x => x)
                .Must(x => x.WeightKilos.HasValue || x.Quantity.HasValue)
                .WithMessage("Must specify either WeightKilos or Quantity");
        });

        // Validación de pago inicial (si se proporciona)
        When(x => x.InitialPayment != null, () =>
        {
            RuleFor(x => x.InitialPayment!.Amount)
                .GreaterThan(0)
                .WithMessage("El monto del pago inicial debe ser mayor a cero");

            RuleFor(x => x.InitialPayment!.PaymentMethodId)
                .GreaterThan(0)
                .WithMessage("El método de pago debe ser válido");

            RuleFor(x => x.InitialPayment!.Reference)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.InitialPayment?.Reference))
                .WithMessage("La referencia no puede exceder 100 caracteres");
        });
    }
}
