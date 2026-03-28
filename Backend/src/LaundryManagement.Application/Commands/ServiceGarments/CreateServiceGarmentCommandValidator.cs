using FluentValidation;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Validador para el comando CreateServiceGarment
/// </summary>
public sealed class CreateServiceGarmentCommandValidator : AbstractValidator<CreateServiceGarmentCommand>
{
    public CreateServiceGarmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del tipo de prenda es requerido")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("La descripción no puede exceder 500 caracteres");
    }
}
