using FluentValidation;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Validador para CreateClientCommand usando FluentValidation
/// </summary>
public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        // Validación de nombre
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido")
            .MinimumLength(3)
            .WithMessage("El nombre debe tener al menos 3 caracteres")
            .MaximumLength(200)
            .WithMessage("El nombre no puede exceder 200 caracteres");

        // Validación de teléfono
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("El teléfono es requerido")
            .Matches(@"^\d{10}$")
            .WithMessage("El teléfono debe contener exactamente 10 dígitos");

        // Validación de email (solo si se proporciona)
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("El formato del email no es válido")
                .MaximumLength(255)
                .WithMessage("El email no puede exceder 255 caracteres");
        });

        // Validación de RFC (solo si se proporciona)
        When(x => !string.IsNullOrWhiteSpace(x.Rfc), () =>
        {
            RuleFor(x => x.Rfc)
                .Matches(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$")
                .WithMessage("El formato del RFC no es válido");
        });

        // Validación de límite de crédito (solo si se proporciona)
        When(x => x.CreditLimit.HasValue, () =>
        {
            RuleFor(x => x.CreditLimit!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El límite de crédito debe ser mayor o igual a cero");
        });
    }
}
