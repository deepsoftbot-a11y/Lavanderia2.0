namespace LaundryManagement.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user lacks required permissions for an operation
/// </summary>
public sealed class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED_ERROR", 403)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, "UNAUTHORIZED_ERROR", 403, innerException)
    {
    }
}
