using LaundryManagement.Domain.Aggregates.Roles;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Repositories;

public sealed class RoleRepositoryPure : IRoleRepository
{
    private readonly LaundryDbContext _context;

    public RoleRepositoryPure(LaundryDbContext context)
    {
        _context = context;
    }

    public async Task<RolePure?> GetByIdAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Roles
            .Include(r => r.RolesPermisos)
            .FirstOrDefaultAsync(r => r.RolId == id.Value, cancellationToken);

        return entity == null ? null : RoleMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<RolePure>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles
            .Include(r => r.RolesPermisos)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(r => r.Activo == isActive.Value);

        var entities = await query.OrderBy(r => r.NombreRol).ToListAsync(cancellationToken);
        return entities.Select(RoleMapper.ToDomain);
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.Where(r => r.NombreRol.ToLower() == name.ToLower().Trim());

        if (excludeId.HasValue)
            query = query.Where(r => r.RolId != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<RolePure> AddAsync(RolePure role, CancellationToken cancellationToken = default)
    {
        var entity = RoleMapper.ToInfrastructure(role);
        await _context.Roles.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Insert permission assignments
        foreach (var permId in role.PermissionIds)
        {
            await _context.RolesPermisos.AddAsync(new RolesPermiso
            {
                RolId = entity.RolId,
                PermisoId = permId
            }, cancellationToken);
        }

        if (role.PermissionIds.Any())
            await _context.SaveChangesAsync(cancellationToken);

        role.SetId(RoleId.From(entity.RolId));
        return role;
    }

    public async Task UpdateAsync(RolePure role, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Roles
            .Include(r => r.RolesPermisos)
            .FirstOrDefaultAsync(r => r.RolId == role.Id.Value, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException($"Rol con ID {role.Id.Value} no encontrado");

        entity.NombreRol = role.Name;
        entity.Descripcion = role.Description;
        entity.Activo = role.IsActive;

        // Sync permissions: remove all then re-insert
        _context.RolesPermisos.RemoveRange(entity.RolesPermisos);

        foreach (var permId in role.PermissionIds)
        {
            entity.RolesPermisos.Add(new RolesPermiso
            {
                RolId = entity.RolId,
                PermisoId = permId
            });
        }

        _context.Roles.Update(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<PermissionInfo>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permisos = await _context.Permisos
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.NombrePermiso)
            .ToListAsync(cancellationToken);

        return permisos.Select(p => new PermissionInfo(p.PermisoId, p.NombrePermiso, p.Modulo, p.Seccion, p.Etiqueta, p.Descripcion));
    }

    public async Task<IEnumerable<PermissionInfo>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var permisos = await _context.RolesPermisos
            .Where(rp => rp.RolId == roleId)
            .Include(rp => rp.Permiso)
            .Select(rp => rp.Permiso)
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.NombrePermiso)
            .ToListAsync(cancellationToken);

        return permisos.Select(p => new PermissionInfo(p.PermisoId, p.NombrePermiso, p.Modulo, p.Seccion, p.Etiqueta, p.Descripcion));
    }
}
