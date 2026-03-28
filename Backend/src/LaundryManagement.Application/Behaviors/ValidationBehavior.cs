using FluentValidation;
using LaundryManagement.Domain.Exceptions;
using MediatR;

namespace LaundryManagement.Application.Behaviors;

/// <summary>
/// Pipeline behavior de MediatR que ejecuta automáticamente los validadores
/// de FluentValidation antes de cada handler.
/// Si hay errores de validación, lanza ValidationException que el
/// GlobalExceptionHandlerMiddleware convierte en respuesta HTTP 400.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si no existen validadores para este comando, pasar directamente al handler
        if (!_validators.Any())
            return await next();

        // Ejecutar todos los validadores
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        // Recopilar todos los errores de validación
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // Si hay errores, lanzar ValidationException con formato Dictionary<string, string[]>
        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(f => f.ErrorMessage).ToArray()
                );

            throw new LaundryManagement.Domain.Exceptions.ValidationException(errors);
        }

        // Sin errores → continuar al handler
        return await next();
    }
}
