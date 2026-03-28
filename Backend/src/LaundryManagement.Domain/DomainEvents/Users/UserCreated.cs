using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Users;

/// <summary>
/// Domain event raised when a new user is created
/// </summary>
public sealed class UserCreated : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }
    public string Email { get; }

    public UserCreated(int userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
    }
}
