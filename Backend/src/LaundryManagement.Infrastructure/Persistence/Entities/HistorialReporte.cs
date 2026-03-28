using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class HistorialReporte
{
    public int HistorialReporteId { get; set; }

    public int ConfigReporteId { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string Estado { get; set; } = null!;

    public string? RutaArchivo { get; set; }

    public string? MensajeError { get; set; }

    public virtual ConfiguracionReporte ConfigReporte { get; set; } = null!;
}
