namespace LaundryManagement.Domain.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message)
        : base(message, "CONFLICT", 409)
    {
    }
}
