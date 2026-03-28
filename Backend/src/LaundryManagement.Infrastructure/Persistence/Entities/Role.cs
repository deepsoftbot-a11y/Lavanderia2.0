using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Role
{
    public int RolId { get; set; }

    public string NombreRol { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<RolesPermiso> RolesPermisos { get; set; } = new List<RolesPermiso>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
