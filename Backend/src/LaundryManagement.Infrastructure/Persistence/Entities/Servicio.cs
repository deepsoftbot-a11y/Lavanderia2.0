using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Servicio
{
    public int ServicioId { get; set; }

    public int CategoriaId { get; set; }

    public string CodigoServicio { get; set; } = null!;

    public string NombreServicio { get; set; } = null!;

    public string TipoCobroServicio { get; set; } = null!;

    public decimal? PrecioPorKilo { get; set; }

    public decimal? PesoMinimo { get; set; }

    public decimal? PesoMaximo { get; set; }

    public string? Descripcion { get; set; }

    public int? TiempoEstimado { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<OrdenesDetalle> OrdenesDetalles { get; set; } = new List<OrdenesDetalle>();

    public virtual ICollection<ServiciosPrenda> ServiciosPrenda { get; set; } = new List<ServiciosPrenda>();
}
