using FluentValidation;

namespace LaundryManagement.Application.Commands.CashClosings;

/// <summary>
/// Validador para el comando de creación de corte de caja
/// </summary>
public class CreateCashClosingCommandValidator : AbstractValidator<CreateCashClosingCommand>
{
    public CreateCashClosingCommandValidator()
    {
        RuleFor(x => x.CajeroID)
            .GreaterThan(0)
            .WithMessage("El CajeroID debe ser válido");

        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida")
            .GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio");

        RuleFor(x => x.FechaCorte)
            .NotEmpty()
            .WithMessage("La fecha del corte es requerida");

        RuleFor(x => x.TotalDeclaradoEfectivo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total declarado en efectivo no puede ser negativo");

        RuleFor(x => x.TotalDeclaradoTarjeta)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total declarado en tarjeta no puede ser negativo");

        RuleFor(x => x.TotalDeclaradoTransferencia)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total declarado en transferencia no puede ser negativo");

        RuleFor(x => x.TotalDeclaradoOtros)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total declarado en otros no puede ser negativo");

        RuleFor(x => x.TotalEsperadoEfectivo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total esperado en efectivo no puede ser negativo");

        RuleFor(x => x.TotalEsperadoTarjeta)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total esperado en tarjeta no puede ser negativo");

        RuleFor(x => x.TotalEsperadoTransferencia)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total esperado en transferencia no puede ser negativo");

        RuleFor(x => x.TotalEsperadoOtros)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El total esperado en otros no puede ser negativo");
    }
}
