using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Permiso
{
    public int PermisoId { get; set; }

    public string NombrePermiso { get; set; } = null!;

    public string Modulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<RolesPermiso> RolesPermisos { get; set; } = new List<RolesPermiso>();
}
