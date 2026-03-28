using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class PagosDetalle
{
    public int PagoDetalleId { get; set; }

    public int PagoId { get; set; }

    public int MetodoPagoId { get; set; }

    public decimal MontoPagado { get; set; }

    public string? Referencia { get; set; }

    public virtual MetodosPago MetodoPago { get; set; } = null!;

    public virtual Pago Pago { get; set; } = null!;
}
