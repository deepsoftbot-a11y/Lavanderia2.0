using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Users;

/// <summary>
/// Domain event raised when a user is deactivated
/// </summary>
public sealed class UserDeactivated : DomainEvent
{
    public int UserId { get; }
    public int DeactivatedBy { get; }

    public UserDeactivated(int userId, int deactivatedBy)
    {
        UserId = userId;
        DeactivatedBy = deactivatedBy;
    }
}
