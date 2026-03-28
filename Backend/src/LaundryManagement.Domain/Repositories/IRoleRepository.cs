using LaundryManagement.Domain.Aggregates.Roles;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

public interface IRoleRepository
{
    Task<RolePure?> GetByIdAsync(RoleId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RolePure>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<RolePure> AddAsync(RolePure role, CancellationToken cancellationToken = default);
    Task UpdateAsync(RolePure role, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<PermissionInfo>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PermissionInfo>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
}
