using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Descuento
{
    public int DescuentoId { get; set; }

    public string NombreDescuento { get; set; } = null!;

    public string TipoDescuento { get; set; } = null!;

    public decimal Valor { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<OrdenesDescuento> OrdenesDescuentos { get; set; } = new List<OrdenesDescuento>();
}
