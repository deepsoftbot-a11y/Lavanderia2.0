using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Pago
{
    public int PagoId { get; set; }

    public string FolioPago { get; set; } = null!;

    public int OrdenId { get; set; }

    public DateTime FechaPago { get; set; }

    public decimal MontoPago { get; set; }

    public int RecibioPor { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? CanceladoEn { get; set; }

    public int? CanceladoPor { get; set; }

    public virtual Ordene Orden { get; set; } = null!;

    public virtual ICollection<PagosDetalle> PagosDetalles { get; set; } = new List<PagosDetalle>();

    public virtual Usuario RecibioPorNavigation { get; set; } = null!;
}
