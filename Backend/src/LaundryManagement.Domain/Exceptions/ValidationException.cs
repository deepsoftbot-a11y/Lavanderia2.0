namespace LaundryManagement.Domain.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR", 400)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Uno o más errores de validación ocurrieron", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }
}
