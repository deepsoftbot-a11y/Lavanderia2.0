using FluentValidation;

namespace LaundryManagement.Application.Commands.Payments;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("ID de orden inválido");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a 0");
        RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Método de pago inválido");
        RuleFor(x => x.ReceivedBy).GreaterThan(0).WithMessage("Usuario que recibe inválido");
        RuleFor(x => x.PaidAt).NotEmpty().WithMessage("La fecha de pago es requerida");
    }
}
