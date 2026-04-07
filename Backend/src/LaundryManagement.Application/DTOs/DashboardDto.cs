namespace LaundryManagement.Application.DTOs;

public sealed record DashboardDto
{
    public DashboardKPIsDto Kpis { get; init; } = null!;
    public DashboardChartsDto Charts { get; init; } = null!;
}

public sealed record DashboardKPIsDto
{
    public decimal IngresosTotales { get; init; }
    public decimal TicketPromedio { get; init; }
    public decimal TotalDescuentos { get; init; }
    public List<IngresoPorMetodoDto> IngresosPorMetodo { get; init; } = new();
    public int OrdenesAtrasadas { get; init; }
    public OrdenesPendientesPagarDto OrdenesPendientesPagar { get; init; } = null!;
    public int ClientesNuevos { get; init; }
    public ClienteTopDto? ClienteTop { get; init; }
    public decimal TotalCorteCaja { get; init; }
    public int Diferencias { get; init; }
    public int Transacciones { get; init; }
}

public sealed record IngresoPorMetodoDto
{
    public string Metodo { get; init; } = null!;
    public decimal Total { get; init; }
}

public sealed record OrdenesPendientesPagarDto
{
    public int Cantidad { get; init; }
    public decimal Total { get; init; }
}

public sealed record ClienteTopDto
{
    public string Nombre { get; init; } = null!;
    public int Ordenes { get; init; }
}

public sealed record DashboardChartsDto
{
    public List<IngresoPorDiaDto> IngresosPorDia { get; init; } = new();
    public List<OrdenesPorEstadoDto> OrdenesPorEstado { get; init; } = new();
    public List<IngresoPorServicioDto> IngresosPorServicio { get; init; } = new();
    public List<IngresoPorCategoriaDto> IngresosPorCategoria { get; init; } = new();
    public ComparativaSemanalDto ComparativaSemanal { get; init; } = null!;
}

public sealed record IngresoPorDiaDto
{
    public DateTime Fecha { get; init; }
    public decimal Ingresos { get; init; }
    public int Ordenes { get; init; }
}

public sealed record OrdenesPorEstadoDto
{
    public string Estado { get; init; } = null!;
    public int Cantidad { get; init; }
}

public sealed record IngresoPorServicioDto
{
    public string Servicio { get; init; } = null!;
    public decimal Total { get; init; }
}

public sealed record IngresoPorCategoriaDto
{
    public string Categoria { get; init; } = null!;
    public decimal Total { get; init; }
}

public sealed record ComparativaSemanalDto
{
    public decimal SemanaActual { get; init; }
    public decimal SemanaAnterior { get; init; }
}
