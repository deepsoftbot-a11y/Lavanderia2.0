using FluentValidation;

namespace LaundryManagement.Application.Commands.Categories;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("El ID de la categoría debe ser válido");

        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("La descripción no puede exceder 255 caracteres")
            .When(x => x.Description != null);
    }
}
