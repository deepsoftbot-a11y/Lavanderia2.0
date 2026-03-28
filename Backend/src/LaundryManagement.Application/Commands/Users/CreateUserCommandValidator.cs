using FluentValidation;

namespace LaundryManagement.Application.Commands.Users;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es requerido")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no tiene un formato válido")
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("El ID de rol debe ser mayor a 0")
            .When(x => x.RoleId.HasValue);
    }
}
