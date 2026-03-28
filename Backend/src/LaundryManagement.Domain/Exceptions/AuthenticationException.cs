namespace LaundryManagement.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication fails (invalid credentials)
/// </summary>
public sealed class AuthenticationException : BaseException
{
    public AuthenticationException(string message)
        : base(message, "AUTHENTICATION_ERROR", 401)
    {
    }

    public AuthenticationException(string message, Exception innerException)
        : base(message, "AUTHENTICATION_ERROR", 401, innerException)
    {
    }
}
