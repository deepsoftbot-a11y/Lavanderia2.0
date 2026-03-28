namespace LaundryManagement.Application.DTOs.Reports;

public record CashClosingReportData(
    CashierInfo      Cashier,
    VentasDiaSection VentasDia,
    BalanzaSection   Balanza,
    CorteCajaSection CorteCaja
);

public record CashierInfo(
    string   Nombre,
    string   Turno,
    DateTime Fecha
);

public record VentasDiaSection(
    decimal Efectivo,
    decimal Tarjeta,
    decimal Transferencia,
    decimal Otros,
    decimal TotalVentas
);

public record BalanzaSection(
    decimal PendienteHoy,
    decimal AcumuladoAyer,
    decimal NuevoAcumulado
);

public record CorteCajaSection(
    decimal FondoInicial,
    decimal SistemaEfectivo,
    decimal RetiroEfectivo,
    decimal AjusteCaja,
    decimal FondoFinal
);
