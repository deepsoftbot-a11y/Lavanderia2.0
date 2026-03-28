using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.DomainEvents.Users;

/// <summary>
/// Domain event raised when a user is activated
/// </summary>
public sealed class UserActivated : DomainEvent
{
    public int UserId { get; }
    public int ActivatedBy { get; }

    public UserActivated(int userId, int activatedBy)
    {
        UserId = userId;
        ActivatedBy = activatedBy;
    }
}
