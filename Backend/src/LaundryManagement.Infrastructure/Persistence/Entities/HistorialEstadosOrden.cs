using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class HistorialEstadosOrden
{
    public int HistorialEstadoId { get; set; }

    public int OrdenId { get; set; }

    public int EstadoOrdenId { get; set; }

    public DateTime FechaCambio { get; set; }

    public int CambiadoPor { get; set; }

    public string? Comentarios { get; set; }

    public virtual Usuario CambiadoPorNavigation { get; set; } = null!;

    public virtual EstadosOrden EstadoOrden { get; set; } = null!;

    public virtual Ordene Orden { get; set; } = null!;
}
