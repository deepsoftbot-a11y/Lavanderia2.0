using FluentValidation;

namespace LaundryManagement.Application.Commands.Permissions;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del permiso debe ser mayor a 0");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .When(x => x.Name != null);

        RuleFor(x => x.Module)
            .MaximumLength(50).WithMessage("El módulo no puede exceder 50 caracteres")
            .When(x => x.Module != null);

        RuleFor(x => x.Section)
            .MaximumLength(50).WithMessage("La sección no puede exceder 50 caracteres")
            .When(x => x.Section != null);

        RuleFor(x => x.Label)
            .MaximumLength(150).WithMessage("La etiqueta no puede exceder 150 caracteres")
            .When(x => x.Label != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => x.Description != null);
    }
}
