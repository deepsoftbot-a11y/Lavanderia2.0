using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class AuditoriaGeneral
{
    public long AuditoriaId { get; set; }

    public string Tabla { get; set; } = null!;

    public string Operacion { get; set; } = null!;

    public int RegistroId { get; set; }

    public string? ValoresAnteriores { get; set; }

    public string? ValoresNuevos { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaOperacion { get; set; }

    public string? DireccionIp { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
