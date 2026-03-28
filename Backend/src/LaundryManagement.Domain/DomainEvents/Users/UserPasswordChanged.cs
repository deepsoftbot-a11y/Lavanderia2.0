using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Users;

/// <summary>
/// Domain event raised when a user's password is changed
/// </summary>
public sealed class UserPasswordChanged : DomainEvent
{
    public int UserId { get; }
    public int ChangedBy { get; }

    public UserPasswordChanged(int userId, int changedBy)
    {
        UserId = userId;
        ChangedBy = changedBy;
    }
}
