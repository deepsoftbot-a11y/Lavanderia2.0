using FluentValidation;

namespace LaundryManagement.Application.Commands.Users;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID de usuario debe ser mayor a 0");

        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres")
            .When(x => x.FullName != null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido")
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres")
            .When(x => x.Email != null);

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .When(x => x.Password != null);

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("El ID de rol debe ser mayor a 0")
            .When(x => x.RoleId.HasValue);
    }
}
