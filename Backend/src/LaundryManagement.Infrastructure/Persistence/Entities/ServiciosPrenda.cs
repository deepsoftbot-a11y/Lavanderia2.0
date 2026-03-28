using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class ServiciosPrenda
{
    public int ServicioPrendaId { get; set; }

    public int ServicioId { get; set; }

    public int TipoPrendaId { get; set; }

    public decimal PrecioUnitario { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual ICollection<CombosDetalle> CombosDetalles { get; set; } = new List<CombosDetalle>();

    public virtual ICollection<OrdenesDetalle> OrdenesDetalles { get; set; } = new List<OrdenesDetalle>();

    public virtual Servicio Servicio { get; set; } = null!;

    public virtual TiposPrendum TipoPrenda { get; set; } = null!;
}
