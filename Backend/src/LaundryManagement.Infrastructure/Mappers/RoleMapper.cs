using LaundryManagement.Domain.Aggregates.Roles;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

public static class RoleMapper
{
    public static RolePure ToDomain(Role entity)
    {
        var permissionIds = entity.RolesPermisos
            .Select(rp => rp.PermisoId)
            .ToList();

        return RolePure.Reconstitute(
            id: RoleId.From(entity.RolId),
            name: entity.NombreRol,
            description: entity.Descripcion,
            isActive: entity.Activo,
            permissionIds: permissionIds
        );
    }

    public static Role ToInfrastructure(RolePure role)
    {
        return new Role
        {
            RolId = role.Id.Value,
            NombreRol = role.Name,
            Descripcion = role.Description,
            Activo = role.IsActive
        };
    }
}
