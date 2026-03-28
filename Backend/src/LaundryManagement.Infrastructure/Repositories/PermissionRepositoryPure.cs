using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Repositories;

public sealed class PermissionRepositoryPure : IPermissionRepository
{
    private readonly LaundryDbContext _context;

    public PermissionRepositoryPure(LaundryDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Permisos.FindAsync(new object[] { id }, cancellationToken);
        return entity == null ? null : ToPermissionInfo(entity);
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Permisos.Where(p => p.NombrePermiso.ToLower() == name.ToLower().Trim());
        if (excludeId.HasValue)
            query = query.Where(p => p.PermisoId != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsAssignedToRolesAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.RolesPermisos
            .AnyAsync(rp => rp.PermisoId == permissionId, cancellationToken);
    }

    public async Task<PermissionInfo> AddAsync(string name, string module, string? description, CancellationToken cancellationToken = default)
    {
        var entity = new Permiso
        {
            NombrePermiso = name.Trim(),
            Modulo = module.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };

        await _context.Permisos.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToPermissionInfo(entity);
    }

    public async Task<PermissionInfo> UpdateAsync(int id, string? name, string? module, string? description, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Permisos.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new NotFoundException($"Permiso con ID {id} no encontrado");

        if (name != null)
            entity.NombrePermiso = name.Trim();

        if (module != null)
            entity.Modulo = module.Trim();

        if (description != null)
            entity.Descripcion = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        _context.Permisos.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToPermissionInfo(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Permisos.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new NotFoundException($"Permiso con ID {id} no encontrado");

        _context.Permisos.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static PermissionInfo ToPermissionInfo(Permiso entity) =>
        new(entity.PermisoId, entity.NombrePermiso, entity.Modulo, entity.Descripcion);
}
