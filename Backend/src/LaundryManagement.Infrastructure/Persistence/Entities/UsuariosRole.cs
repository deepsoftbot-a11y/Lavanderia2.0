using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class UsuariosRole
{
    public int UsuarioRolId { get; set; }

    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public virtual Role Rol { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
