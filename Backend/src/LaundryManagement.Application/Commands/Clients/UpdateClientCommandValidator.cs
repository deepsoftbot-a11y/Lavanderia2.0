using FluentValidation;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Validador para UpdateClientCommand usando FluentValidation
/// </summary>
public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        // Validación de ClientId
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage("El ID del cliente debe ser válido");

        // Validación de nombre (solo si se proporciona)
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres")
                .MaximumLength(200)
                .WithMessage("El nombre no puede exceder 200 caracteres");
        });

        // Validación de teléfono (solo si se proporciona)
        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(\+52)?[1-9]\d{9}$")
                .WithMessage("Formato de teléfono inválido");
        });

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
