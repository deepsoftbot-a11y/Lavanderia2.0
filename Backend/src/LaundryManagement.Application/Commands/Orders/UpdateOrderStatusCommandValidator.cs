using FluentValidation;

namespace LaundryManagement.Application.Commands.Orders;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("ID de orden inválido");
        RuleFor(x => x.NewStatusId).GreaterThan(0).WithMessage("El nuevo estado debe ser válido");
    }
}
