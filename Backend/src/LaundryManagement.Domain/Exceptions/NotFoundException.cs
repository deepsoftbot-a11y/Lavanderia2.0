namespace LaundryManagement.Domain.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} con ID {id} no fue encontrado", "NOT_FOUND", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND", 404)
    {
    }
}
