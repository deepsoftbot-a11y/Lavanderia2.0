using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Ordene
{
    public int OrdenId { get; set; }

    public string FolioOrden { get; set; } = null!;

    public int ClienteId { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public DateTime FechaPrometida { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public int EstadoOrdenId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    public string? Ubicaciones { get; set; }

    public int RecibidoPor { get; set; }

    public int? EntregadoPor { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Usuario? EntregadoPorNavigation { get; set; }

    public virtual EstadosOrden EstadoOrden { get; set; } = null!;

    public virtual ICollection<HistorialEstadosOrden> HistorialEstadosOrdens { get; set; } = new List<HistorialEstadosOrden>();

    public virtual ICollection<OrdenesDescuento> OrdenesDescuentos { get; set; } = new List<OrdenesDescuento>();

    public virtual ICollection<OrdenesDetalle> OrdenesDetalles { get; set; } = new List<OrdenesDetalle>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Usuario RecibidoPorNavigation { get; set; } = null!;
}
