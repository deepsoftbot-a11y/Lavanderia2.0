using LaundryManagement.Domain.Aggregates.Users;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate (defined in Domain, implemented in Infrastructure)
/// </summary>
public interface IUserRepository
{
    // Queries
    Task<UserPure?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<UserPure?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);
    Task<UserPure?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Username username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserPure>> GetAllAsync(string? search = null, bool? isActive = null, int? roleId = null, string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default);

    // Commands
    Task<UserPure> AddAsync(UserPure user, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserPure user, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
