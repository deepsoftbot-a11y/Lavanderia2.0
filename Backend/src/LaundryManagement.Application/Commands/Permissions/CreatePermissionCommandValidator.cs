using FluentValidation;

namespace LaundryManagement.Application.Commands.Permissions;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del permiso es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("El módulo es requerido")
            .MaximumLength(50).WithMessage("El módulo no puede exceder 50 caracteres");

        RuleFor(x => x.Section)
            .NotEmpty().WithMessage("La sección es requerida")
            .MaximumLength(50).WithMessage("La sección no puede exceder 50 caracteres");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("La etiqueta es requerida")
            .MaximumLength(150).WithMessage("La etiqueta no puede exceder 150 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => x.Description != null);
    }
}
