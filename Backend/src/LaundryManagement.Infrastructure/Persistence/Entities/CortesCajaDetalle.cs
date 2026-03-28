using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class CortesCajaDetalle
{
    public int CorteDetalleId { get; set; }

    public int CorteId { get; set; }

    public int MetodoPagoId { get; set; }

    public int NumeroTransacciones { get; set; }

    public decimal TotalEsperado { get; set; }

    public decimal TotalDeclarado { get; set; }

    public decimal? Diferencia { get; set; }

    public virtual CortesCaja Corte { get; set; } = null!;

    public virtual MetodosPago MetodoPago { get; set; } = null!;
}
