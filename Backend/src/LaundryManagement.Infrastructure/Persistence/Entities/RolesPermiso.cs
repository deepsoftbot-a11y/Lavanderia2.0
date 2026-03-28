using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class RolesPermiso
{
    public int RolPermisoId { get; set; }

    public int RolId { get; set; }

    public int PermisoId { get; set; }

    public virtual Permiso Permiso { get; set; } = null!;

    public virtual Role Rol { get; set; } = null!;
}
