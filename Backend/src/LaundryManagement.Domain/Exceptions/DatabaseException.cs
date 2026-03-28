namespace LaundryManagement.Domain.Exceptions;

public class DatabaseException : BaseException
{
    public DatabaseException(string message)
        : base(message, "DATABASE_ERROR", 500)
    {
    }

    public DatabaseException(string message, Exception innerException)
        : base(message, "DATABASE_ERROR", 500, innerException)
    {
    }
}
