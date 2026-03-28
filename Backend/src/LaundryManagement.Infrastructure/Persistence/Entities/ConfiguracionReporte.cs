using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class ConfiguracionReporte
{
    public int ConfigReporteId { get; set; }

    public string NombreReporte { get; set; } = null!;

    public string TipoReporte { get; set; } = null!;

    public string Frecuencia { get; set; } = null!;

    public string FormatoExportacion { get; set; } = null!;

    public string? DestinatariosEmail { get; set; }

    public string? HoraEnvio { get; set; }

    public bool Activo { get; set; }

    public string? ParametrosJson { get; set; }

    public virtual ICollection<HistorialReporte> HistorialReportes { get; set; } = new List<HistorialReporte>();
}
