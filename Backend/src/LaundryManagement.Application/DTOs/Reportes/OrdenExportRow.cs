namespace LaundryManagement.Application.DTOs.Reportes;

public sealed record OrdenExportRow(
    string Folio,
    string Cliente,
    DateTime FechaCreacion,
    DateTime FechaPrometida,
    string EstadoOrden,
    string EstadoPago,
    decimal Subtotal,
    decimal Descuento,
    decimal Total,
    decimal Pagado,
    decimal Saldo
);
