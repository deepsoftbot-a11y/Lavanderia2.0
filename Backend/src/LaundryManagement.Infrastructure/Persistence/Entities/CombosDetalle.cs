using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class CombosDetalle
{
    public int ComboDetalleId { get; set; }

    public int ComboId { get; set; }

    public int ServicioPrendaId { get; set; }

    public int CantidadMinima { get; set; }

    public virtual Combo Combo { get; set; } = null!;

    public virtual ServiciosPrenda ServicioPrenda { get; set; } = null!;
}
