using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class MetodosPago
{
    public int MetodoPagoId { get; set; }

    public string NombreMetodo { get; set; } = null!;

    public bool RequiereReferencia { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<CortesCajaDetalle> CortesCajaDetalles { get; set; } = new List<CortesCajaDetalle>();

    public virtual ICollection<PagosDetalle> PagosDetalles { get; set; } = new List<PagosDetalle>();
}
