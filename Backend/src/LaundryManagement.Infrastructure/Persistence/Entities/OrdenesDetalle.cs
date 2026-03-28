using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class OrdenesDetalle
{
    public int OrdenDetalleId { get; set; }

    public int OrdenId { get; set; }

    public int NumeroLinea { get; set; }

    public int ServicioId { get; set; }

    public int? ServicioPrendaId { get; set; }

    public decimal? PesoKilos { get; set; }

    public int? Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DescuentoLinea { get; set; }

    public decimal TotalLinea { get; set; }

    public string? Observaciones { get; set; }

    public virtual Ordene Orden { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;

    public virtual ServiciosPrenda? ServicioPrenda { get; set; }
}
