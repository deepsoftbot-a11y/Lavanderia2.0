using FluentValidation;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Validador para el comando UpdateServiceGarment
/// </summary>
public sealed class UpdateServiceGarmentCommandValidator : AbstractValidator<UpdateServiceGarmentCommand>
{
    public UpdateServiceGarmentCommandValidator()
    {
        RuleFor(x => x.ServiceGarmentId)
            .GreaterThan(0)
            .WithMessage("El ID del tipo de prenda debe ser mayor a cero");

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
