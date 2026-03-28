namespace LaundryManagement.Domain.Exceptions;

public abstract class BaseException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    protected BaseException(string message, string code, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    protected BaseException(string message, string code, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
