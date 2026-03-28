using FluentValidation;

namespace LaundryManagement.Application.Commands.Auth;

/// <summary>
/// Validator for LoginCommand
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es requerido")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
    }
}
