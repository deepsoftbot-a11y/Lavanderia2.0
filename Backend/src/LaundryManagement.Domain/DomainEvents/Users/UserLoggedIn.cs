using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Users;

/// <summary>
/// Domain event raised when a user successfully logs in
/// </summary>
public sealed class UserLoggedIn : DomainEvent
{
    public int UserId { get; }
    public string Username { get; }

    public UserLoggedIn(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}
