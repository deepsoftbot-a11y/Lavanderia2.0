using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Combo
{
    public int ComboId { get; set; }

    public string NombreCombo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PorcentajeDescuento { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<CombosDetalle> CombosDetalles { get; set; } = new List<CombosDetalle>();

    public virtual ICollection<OrdenesDescuento> OrdenesDescuentos { get; set; } = new List<OrdenesDescuento>();
}
