using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class TiposPrendum
{
    public int TipoPrendaId { get; set; }

    public string NombrePrenda { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ServiciosPrenda> ServiciosPrenda { get; set; } = new List<ServiciosPrenda>();
}
