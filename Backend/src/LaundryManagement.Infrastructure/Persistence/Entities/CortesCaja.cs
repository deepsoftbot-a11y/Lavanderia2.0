using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class CortesCaja
{
    public int CorteId { get; set; }

    public string FolioCorte { get; set; } = null!;

    public int CajeroId { get; set; }

    public string? TurnoDescripcion { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public DateTime FechaCorte { get; set; }

    public decimal? FondoInicial { get; set; }

    public decimal TotalEsperadoEfectivo { get; set; }

    public decimal TotalEsperadoTarjeta { get; set; }

    public decimal TotalEsperadoTransferencia { get; set; }

    public decimal TotalEsperadoOtros { get; set; }

    public decimal TotalEsperado { get; set; }

    public decimal TotalDeclaradoEfectivo { get; set; }

    public decimal TotalDeclaradoTarjeta { get; set; }

    public decimal TotalDeclaradoTransferencia { get; set; }

    public decimal TotalDeclaradoOtros { get; set; }

    public decimal TotalDeclarado { get; set; }

    public decimal? DiferenciaInicialEfectivo { get; set; }

    public decimal? DiferenciaInicialTarjeta { get; set; }

    public decimal? DiferenciaInicialTransferencia { get; set; }

    public decimal? DiferenciaInicialOtros { get; set; }

    public decimal? DiferenciaInicial { get; set; }

    public decimal MontoAjuste { get; set; }

    public string? MotivoAjuste { get; set; }

    public DateTime? FechaAjuste { get; set; }

    public decimal? DiferenciaFinal { get; set; }

    public int NumeroTransacciones { get; set; }

    public string? Observaciones { get; set; }

    public virtual Usuario Cajero { get; set; } = null!;

    public virtual ICollection<CortesCajaDetalle> CortesCajaDetalles { get; set; } = new List<CortesCajaDetalle>();
}
