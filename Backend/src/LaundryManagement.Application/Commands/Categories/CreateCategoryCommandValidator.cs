using FluentValidation;

namespace LaundryManagement.Application.Commands.Categories;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("La descripción no puede exceder 255 caracteres")
            .When(x => x.Description != null);
    }
}
