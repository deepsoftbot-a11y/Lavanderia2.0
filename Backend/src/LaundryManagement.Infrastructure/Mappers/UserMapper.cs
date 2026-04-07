using LaundryManagement.Domain.Aggregates.Users;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper between UserPure (Domain) and Usuario (Infrastructure)
/// Anti-corruption layer to maintain pure domain separation
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Maps from Infrastructure entity (Usuario) to Domain aggregate (UserPure)
    /// </summary>
    public static UserPure ToDomain(Usuario entity)
    {
        // Map role assignments
        var roleAssignments = entity.UsuariosRoles
            .Select(ur => UserRoleAssignment.Reconstitute(
                id: ur.UsuarioRolId,
                roleId: ur.RolId,
                role: UserRole.From(ur.Rol.NombreRol),
                assignedAt: ur.FechaAsignacion
            ))
            .ToList();

        // Map permissions (flatten from roles)
        var permissions = entity.UsuariosRoles
            .SelectMany(ur => ur.Rol.RolesPermisos.Select(rp => rp.Permiso))
            .Distinct()
            .Select(p => UserPermission.Reconstitute(
                id: p.PermisoId,
                permissionId: p.PermisoId,
                permissionName: p.NombrePermiso,
                module: p.Modulo,
                section: p.Seccion,
                label: p.Etiqueta
            ))
            .ToList();

        // Reconstitute domain aggregate
        return UserPure.Reconstitute(
            id: UserId.From(entity.UsuarioId),
            username: Username.From(entity.NombreUsuario),
            email: Email.From(entity.Email),
            passwordHash: PasswordHash.FromHash(entity.PasswordHash),
            fullName: entity.NombreCompleto,
            isActive: entity.Activo,
            createdAt: entity.FechaCreacion,
            lastLogin: entity.UltimoAcceso,
            createdBy: entity.CreadoPor,
            roleAssignments: roleAssignments,
            permissions: permissions
        );
    }

    /// <summary>
    /// Maps from Domain aggregate (UserPure) to Infrastructure entity (Usuario)
    /// </summary>
    public static Usuario ToInfrastructure(UserPure user)
    {
        return new Usuario
        {
            UsuarioId = user.Id.Value,
            NombreUsuario = user.Username.Value,
            Email = user.Email.Value,
            PasswordHash = user.PasswordHash.Value,
            NombreCompleto = user.FullName,
            Activo = user.IsActive,
            FechaCreacion = user.CreatedAt,
            UltimoAcceso = user.LastLogin,
            CreadoPor = user.CreatedBy
            // Navigation properties are not mapped here (managed by EF Core)
        };
    }
}
