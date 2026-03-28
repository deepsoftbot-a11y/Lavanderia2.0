using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class OrdenesDescuento
{
    public int OrdenDescuentoId { get; set; }

    public int OrdenId { get; set; }

    public int? DescuentoId { get; set; }

    public int? ComboId { get; set; }

    public decimal MontoDescuento { get; set; }

    public string? Justificacion { get; set; }

    public int AplicadoPor { get; set; }

    public virtual Usuario AplicadoPorNavigation { get; set; } = null!;

    public virtual Combo? Combo { get; set; }

    public virtual Descuento? Descuento { get; set; }

    public virtual Ordene Orden { get; set; } = null!;
}
