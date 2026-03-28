using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public int? CreadoPor { get; set; }

    public virtual ICollection<AuditoriaGeneral> AuditoriaGenerals { get; set; } = new List<AuditoriaGeneral>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<CortesCaja> CortesCajas { get; set; } = new List<CortesCaja>();

    public virtual Usuario? CreadoPorNavigation { get; set; }

    public virtual ICollection<HistorialEstadosOrden> HistorialEstadosOrdens { get; set; } = new List<HistorialEstadosOrden>();

    public virtual ICollection<Usuario> InverseCreadoPorNavigation { get; set; } = new List<Usuario>();

    public virtual ICollection<Ordene> OrdeneEntregadoPorNavigations { get; set; } = new List<Ordene>();

    public virtual ICollection<Ordene> OrdeneRecibidoPorNavigations { get; set; } = new List<Ordene>();

    public virtual ICollection<OrdenesDescuento> OrdenesDescuentos { get; set; } = new List<OrdenesDescuento>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual ICollection<UsuariosRole> UsuariosRoles { get; set; } = new List<UsuariosRole>();
}
