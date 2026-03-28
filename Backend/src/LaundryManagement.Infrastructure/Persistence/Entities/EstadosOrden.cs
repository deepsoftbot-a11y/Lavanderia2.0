using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class EstadosOrden
{
    public int EstadoOrdenId { get; set; }

    public string NombreEstado { get; set; } = null!;

    public string? ColorEstado { get; set; }

    public int OrdenProceso { get; set; }

    public virtual ICollection<HistorialEstadosOrden> HistorialEstadosOrdens { get; set; } = new List<HistorialEstadosOrden>();

    public virtual ICollection<Ordene> Ordenes { get; set; } = new List<Ordene>();
}
