using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Repositories;

public interface IPermissionRepository
{
    Task<PermissionInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsAssignedToRolesAsync(int permissionId, CancellationToken cancellationToken = default);
    Task<PermissionInfo> AddAsync(string name, string module, string? description, CancellationToken cancellationToken = default);
    Task<PermissionInfo> AddAsync(string name, string module, string section, string label, string? description, CancellationToken cancellationToken = default);
    Task<PermissionInfo> UpdateAsync(int id, string? name, string? module, string? description, CancellationToken cancellationToken = default);
    Task<PermissionInfo> UpdateAsync(int id, string? name, string? module, string? section, string? label, string? description, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
