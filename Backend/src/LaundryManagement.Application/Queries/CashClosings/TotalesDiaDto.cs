namespace LaundryManagement.Application.Queries.CashClosings;

/// <summary>
/// DTO que representa los totales de pagos del día por método de pago
/// </summary>
public class TotalesDiaDto
{
    /// <summary>
    /// Total en efectivo
    /// </summary>
    public decimal TotalEfectivo { get; set; }

    /// <summary>
    /// Total en tarjeta
    /// </summary>
    public decimal TotalTarjeta { get; set; }

    /// <summary>
    /// Total en transferencia
    /// </summary>
    public decimal TotalTransferencia { get; set; }

    /// <summary>
    /// Total en otros métodos de pago
    /// </summary>
    public decimal TotalOtros { get; set; }

    /// <summary>
    /// Total general pagado
    /// </summary>
    public decimal TotalPagado { get; set; }
}
