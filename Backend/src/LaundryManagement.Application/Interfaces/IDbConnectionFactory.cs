using System.Data;

namespace LaundryManagement.Application.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
