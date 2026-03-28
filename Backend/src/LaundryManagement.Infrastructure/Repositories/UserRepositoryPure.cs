using LaundryManagement.Domain.Aggregates.Users;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Pure repository implementation for User aggregate
/// Implements IUserRepository from Domain layer
/// </summary>
public sealed class UserRepositoryPure : IUserRepository
{
    private readonly LaundryDbContext _context;

    public UserRepositoryPure(LaundryDbContext context)
    {
        _context = context;
    }

    public async Task<UserPure?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Usuarios
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolesPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.UsuarioId == id.Value, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<UserPure?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Usuarios
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolesPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.NombreUsuario == username.Value, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<UserPure?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Usuarios
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolesPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<bool> ExistsAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == username.Value, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email.Value, cancellationToken);
    }

    public async Task<IEnumerable<UserPure>> GetAllAsync(
        string? search = null,
        bool? isActive = null,
        int? roleId = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Usuarios
            .Include(u => u.UsuariosRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolesPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.NombreCompleto.ToLower().Contains(term) ||
                u.NombreUsuario.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.Activo == isActive.Value);

        if (roleId.HasValue)
            query = query.Where(u => u.UsuariosRoles.Any(ur => ur.RolId == roleId.Value));

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLower() switch
        {
            "username" => isDesc ? query.OrderByDescending(u => u.NombreUsuario) : query.OrderBy(u => u.NombreUsuario),
            "createdat" => isDesc ? query.OrderByDescending(u => u.FechaCreacion) : query.OrderBy(u => u.FechaCreacion),
            "lastlogin" => isDesc ? query.OrderByDescending(u => u.UltimoAcceso) : query.OrderBy(u => u.UltimoAcceso),
            _ => isDesc ? query.OrderByDescending(u => u.NombreCompleto) : query.OrderBy(u => u.NombreCompleto)
        };

        var entities = await query.ToListAsync(cancellationToken);
        return entities.Select(UserMapper.ToDomain);
    }

    public async Task<UserPure> AddAsync(UserPure user, CancellationToken cancellationToken = default)
    {
        var entity = UserMapper.ToInfrastructure(user);
        await _context.Usuarios.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        user.SetId(UserId.From(entity.UsuarioId));

        // Insert role assignment if present
        foreach (var assignment in user.RoleAssignments)
        {
            await _context.UsuariosRoles.AddAsync(new UsuariosRole
            {
                UsuarioId = entity.UsuarioId,
                RolId = assignment.RoleId,
                FechaAsignacion = assignment.AssignedAt
            }, cancellationToken);
        }

        if (user.RoleAssignments.Any())
            await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task UpdateAsync(UserPure user, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Usuarios
            .Include(u => u.UsuariosRoles)
            .FirstOrDefaultAsync(u => u.UsuarioId == user.Id.Value, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Usuario con ID {user.Id.Value} no encontrado");
        }

        entity.PasswordHash = user.PasswordHash.Value;
        entity.NombreCompleto = user.FullName;
        entity.Email = user.Email.Value;
        entity.Activo = user.IsActive;
        entity.UltimoAcceso = user.LastLogin;

        // Sync role assignments
        _context.UsuariosRoles.RemoveRange(entity.UsuariosRoles);
        foreach (var assignment in user.RoleAssignments)
        {
            entity.UsuariosRoles.Add(new UsuariosRole
            {
                UsuarioId = entity.UsuarioId,
                RolId = assignment.RoleId,
                FechaAsignacion = assignment.AssignedAt
            });
        }

        _context.Usuarios.Update(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
