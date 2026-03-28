using MediatR;

namespace LaundryManagement.Application.Commands.CashClosings;

/// <summary>
/// Command para crear un nuevo corte de caja
/// </summary>
public sealed record CreateCashClosingCommand : IRequest<CreateCashClosingResponse>
{
    /// <summary>
    /// ID del cajero
    /// </summary>
    public int CajeroID { get; init; }

    /// <summary>
    /// Fecha de inicio del período
    /// </summary>
    public DateTime FechaInicio { get; init; }

    /// <summary>
    /// Fecha de fin del período
    /// </summary>
    public DateTime FechaFin { get; init; }

    /// <summary>
    /// Fecha del corte
    /// </summary>
    public DateTime FechaCorte { get; init; }

    /// <summary>
    /// Descripción del turno
    /// </summary>
    public string TurnoDescripcion { get; init; } = string.Empty;

    #region Totales Esperados (del sistema)

    /// <summary>
    /// Total esperado en efectivo
    /// </summary>
    public decimal TotalEsperadoEfectivo { get; init; }

    /// <summary>
    /// Total esperado en tarjeta
    /// </summary>
    public decimal TotalEsperadoTarjeta { get; init; }

    /// <summary>
    /// Total esperado en transferencia
    /// </summary>
    public decimal TotalEsperadoTransferencia { get; init; }

    /// <summary>
    /// Total esperado en otros métodos
    /// </summary>
    public decimal TotalEsperadoOtros { get; init; }

    /// <summary>
    /// Total general esperado
    /// </summary>
    public decimal TotalEsperado { get; init; }

    #endregion

    #region Totales Declarados (por el cajero)

    /// <summary>
    /// Total declarado en efectivo
    /// </summary>
    public decimal TotalDeclaradoEfectivo { get; init; }

    /// <summary>
    /// Total declarado en tarjeta
    /// </summary>
    public decimal TotalDeclaradoTarjeta { get; init; }

    /// <summary>
    /// Total declarado en transferencia
    /// </summary>
    public decimal TotalDeclaradoTransferencia { get; init; }

    /// <summary>
    /// Total declarado en otros métodos
    /// </summary>
    public decimal TotalDeclaradoOtros { get; init; }

    /// <summary>
    /// Total general declarado
    /// </summary>
    public decimal TotalDeclarado { get; init; }

    #endregion

    #region Ajuste

    /// <summary>
    /// Monto del ajuste
    /// </summary>
    public decimal MontoAjuste { get; init; }

    /// <summary>
    /// Motivo del ajuste
    /// </summary>
    public string? MotivoAjuste { get; init; }

    #endregion

    /// <summary>
    /// Observaciones generales
    /// </summary>
    public string? Observaciones { get; init; }

    /// <summary>
    /// Fondo inicial de caja
    /// </summary>
    public decimal? FondoInicial { get; init; }
}
