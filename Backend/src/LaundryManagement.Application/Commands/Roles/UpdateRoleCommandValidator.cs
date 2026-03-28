using FluentValidation;

namespace LaundryManagement.Application.Commands.Roles;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del rol debe ser mayor a 0");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("El nombre del rol no puede exceder 100 caracteres")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => x.Description != null);
    }
}
