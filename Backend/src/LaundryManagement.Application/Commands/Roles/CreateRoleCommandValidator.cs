using FluentValidation;

namespace LaundryManagement.Application.Commands.Roles;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del rol es requerido")
            .MaximumLength(100).WithMessage("El nombre del rol no puede exceder 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => x.Description != null);

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Los permisos son requeridos");
    }
}
