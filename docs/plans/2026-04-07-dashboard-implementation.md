# Dashboard Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Construir el módulo de Dashboard con métricas KPIs, gráficos y filtro de rango de fechas. Solo accesible para rol admin.

**Architecture:** Backend expone un endpoint único `GET /api/dashboard` con todos los datos. Queries Dapper de solo lectura para métricas. Frontend con Zustand store + componentes React con Recharts.

**Tech Stack:** C# / ASP.NET Core 8 / Dapper — React 19 / TypeScript / Zustand / Recharts / date-fns / Shadcn/ui

---

## Contexto clave

- Tablas disponibles: `Ordenes`, `Pagos`, `PagosDetalle`, `Clientes`, `CortesCaja`, `OrdenesDetalle`, `Servicios`, `Categorias`, `MetodosPago`, `EstadosOrden`.
- Queries Dapper van en `Infrastructure/Services/DashboardService.cs`. SQL usa comillas dobles para identificadores PascalCase.
- `IDbConnectionFactory.CreateConnection()` devuelve `IDbConnection` (no `DbConnection`). Usar `System.Data.IDbConnection` como tipo en los métodos privados — Dapper funciona sobre esa interfaz.
- Frontend: `Dashboard.tsx` ya existe con estructura básica — se modifica, no se reemplaza.
- Fechas por defecto: `fechaInicio` = primer día del mes actual, `fechaFin` = hoy.

---

## Task 1: Backend — DashboardDto

**Files:**
- Create: `Backend/src/LaundryManagement.Application/DTOs/DashboardDto.cs`

### Paso 1: Crear el archivo con todos los DTOs internos

```csharp
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
```

### Paso 2: Build

```bash
cd Backend
dotnet build LaundryManagement.sln 2>&1 | head -20
```

Esperado: sin errores.

### Paso 3: Commit

```bash
git add Backend/src/LaundryManagement.Application/DTOs/DashboardDto.cs
git commit -m "feat(backend): agregar DashboardDto con KPIs y Charts"
```

---

## Task 2: Backend — IDashboardService

**Files:**
- Create: `Backend/src/LaundryManagement.Application/Interfaces/IDashboardService.cs`

### Paso 1: Crear la interfaz

```csharp
using LaundryManagement.Application.DTOs;

namespace LaundryManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default);
}
```

### Paso 2: Build

```bash
cd Backend
dotnet build LaundryManagement.sln 2>&1 | head -10
```

Esperado: sin errores.

### Paso 3: Commit

```bash
git add Backend/src/LaundryManagement.Application/Interfaces/IDashboardService.cs
git commit -m "feat(backend): agregar IDashboardService interface"
```

---

## Task 3: Backend — DashboardService con queries Dapper

**Files:**
- Create: `Backend/src/LaundryManagement.Infrastructure/Services/DashboardService.cs`

### Paso 1: Crear el servicio con todas las queries

```csharp
using System.Data;
using Dapper;
using LaundryManagement.Application.DTOs;
using LaundryManagement.Application.Interfaces;

namespace LaundryManagement.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDbConnectionFactory _db;

    public DashboardService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetDashboardAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();

        var kpis = new DashboardKPIsDto
        {
            IngresosTotales = await GetIngresosTotales(conn, fechaInicio, fechaFin),
            TicketPromedio = await GetTicketPromedio(conn, fechaInicio, fechaFin),
            TotalDescuentos = await GetTotalDescuentos(conn, fechaInicio, fechaFin),
            IngresosPorMetodo = await GetIngresosPorMetodo(conn, fechaInicio, fechaFin),
            OrdenesAtrasadas = await GetOrdenesAtrasadas(conn),
            OrdenesPendientesPagar = await GetOrdenesPendientesPagar(conn, fechaInicio, fechaFin),
            ClientesNuevos = await GetClientesNuevos(conn, fechaInicio, fechaFin),
            ClienteTop = await GetClienteTop(conn, fechaInicio, fechaFin),
            TotalCorteCaja = await GetTotalCorteCaja(conn, fechaInicio, fechaFin),
            Diferencias = await GetDiferencias(conn, fechaInicio, fechaFin),
            Transacciones = await GetTransacciones(conn, fechaInicio, fechaFin),
        };

        var charts = new DashboardChartsDto
        {
            IngresosPorDia = await GetIngresosPorDia(conn, fechaInicio, fechaFin),
            OrdenesPorEstado = await GetOrdenesPorEstado(conn, fechaInicio, fechaFin),
            IngresosPorServicio = await GetIngresosPorServicio(conn, fechaInicio, fechaFin),
            IngresosPorCategoria = await GetIngresosPorCategoria(conn, fechaInicio, fechaFin),
            ComparativaSemanal = await GetComparativaSemanal(conn),
        };

        return new DashboardDto { Kpis = kpis, Charts = charts };
    }

    private async Task<decimal> GetIngresosTotales(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""Total""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<decimal> GetTicketPromedio(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(AVG(""Total""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<decimal> GetTotalDescuentos(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""Descuento""), 0) FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<List<IngresoPorMetodoDto>> GetIngresosPorMetodo(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT m.""NombreMetodo"" AS Metodo, COALESCE(SUM(pd.""MontoPagado""), 0) AS Total
                    FROM ""Pagos"" p
                    JOIN ""PagosDetalle"" pd ON p.""PagoId"" = pd.""PagoId""
                    JOIN ""MetodosPago"" m ON pd.""MetodoPagoId"" = m.""MetodoPagoId""
                    JOIN ""Ordenes"" o ON p.""OrdenId"" = o.""OrdenId""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY m.""NombreMetodo""";
        var result = await conn.QueryAsync<IngresoPorMetodoDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private async Task<int> GetOrdenesAtrasadas(IDbConnection conn)
    {
        var sql = @"SELECT COUNT(*) FROM ""Ordenes"" o
                    WHERE o.""FechaPrometida"" < CURRENT_DATE
                    AND o.""EstadoOrdenId"" NOT IN (4, 5)
                    AND o.""FechaEntrega"" IS NULL";
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    private async Task<OrdenesPendientesPagarDto> GetOrdenesPendientesPagar(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COUNT(*) FROM ""Ordenes"" o
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    AND o.""Total"" > COALESCE((
                        SELECT SUM(p.""MontoPago"") FROM ""Pagos"" p
                        WHERE p.""OrdenId"" = o.""OrdenId"" AND p.""CanceladoEn"" IS NULL
                    ), 0)";
        var cantidad = await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });

        var sql2 = @"SELECT COALESCE(SUM(o.""Total""), 0) FROM ""Ordenes"" o
                     WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                     AND o.""Total"" > COALESCE((
                         SELECT SUM(p.""MontoPago"") FROM ""Pagos"" p
                         WHERE p.""OrdenId"" = o.""OrdenId"" AND p.""CanceladoEn"" IS NULL
                     ), 0)";
        var total = await conn.ExecuteScalarAsync<decimal>(sql2, new { Fi = fi, Ff = ff });

        return new OrdenesPendientesPagarDto { Cantidad = cantidad, Total = total };
    }

    private async Task<int> GetClientesNuevos(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COUNT(*) FROM ""Clientes""
                    WHERE ""FechaRegistro"" >= @Fi AND ""FechaRegistro"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<ClienteTopDto?> GetClienteTop(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT c.""NombreCompleto"" AS Nombre, COUNT(*) AS Ordenes
                    FROM ""Ordenes"" o
                    JOIN ""Clientes"" c ON o.""ClienteId"" = c.""ClienteId""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY c.""ClienteId"", c.""NombreCompleto""
                    ORDER BY COUNT(*) DESC
                    LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<ClienteTopDto>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<decimal> GetTotalCorteCaja(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""TotalDeclarado""), 0) FROM ""CortesCaja""
                    WHERE ""FechaCorte"" >= @Fi AND ""FechaCorte"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<int> GetDiferencias(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COUNT(*) FROM ""CortesCaja""
                    WHERE ""FechaCorte"" >= @Fi AND ""FechaCorte"" < @Ff + INTERVAL '1 day'
                    AND ""DiferenciaFinal"" != 0";
        return await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<int> GetTransacciones(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT COALESCE(SUM(""NumeroTransacciones""), 0) FROM ""CortesCaja""
                    WHERE ""FechaCorte"" >= @Fi AND ""FechaCorte"" < @Ff + INTERVAL '1 day'";
        return await conn.ExecuteScalarAsync<int>(sql, new { Fi = fi, Ff = ff });
    }

    private async Task<List<IngresoPorDiaDto>> GetIngresosPorDia(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT DATE(""FechaRecepcion"") AS Fecha,
                           COALESCE(SUM(""Total""), 0) AS Ingresos,
                           COUNT(*) AS Ordenes
                    FROM ""Ordenes""
                    WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY DATE(""FechaRecepcion"")
                    ORDER BY Fecha";
        var result = await conn.QueryAsync<IngresoPorDiaDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private async Task<List<OrdenesPorEstadoDto>> GetOrdenesPorEstado(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT e.""NombreEstado"" AS Estado, COUNT(*) AS Cantidad
                    FROM ""Ordenes"" o
                    JOIN ""EstadosOrden"" e ON o.""EstadoOrdenId"" = e.""EstadoOrdenId""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY e.""NombreEstado"", e.""OrdenProceso""
                    ORDER BY e.""OrdenProceso""";
        var result = await conn.QueryAsync<OrdenesPorEstadoDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private async Task<List<IngresoPorServicioDto>> GetIngresosPorServicio(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT s.""NombreServicio"" AS Servicio,
                           COALESCE(SUM(od.""Subtotal""), 0) AS Total
                    FROM ""OrdenesDetalle"" od
                    JOIN ""Ordenes"" o ON od.""OrdenId"" = o.""OrdenId""
                    JOIN ""Servicios"" s ON od.""ServicioId"" = s.""ServicioId""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY s.""NombreServicio""
                    ORDER BY Total DESC";
        var result = await conn.QueryAsync<IngresoPorServicioDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private async Task<List<IngresoPorCategoriaDto>> GetIngresosPorCategoria(IDbConnection conn, DateTime fi, DateTime ff)
    {
        var sql = @"SELECT c.""NombreCategoria"" AS Categoria,
                           COALESCE(SUM(od.""Subtotal""), 0) AS Total
                    FROM ""OrdenesDetalle"" od
                    JOIN ""Ordenes"" o ON od.""OrdenId"" = o.""OrdenId""
                    JOIN ""Servicios"" s ON od.""ServicioId"" = s.""ServicioId""
                    JOIN ""Categorias"" c ON s.""CategoriaId"" = c.""CategoriaId""
                    WHERE o.""FechaRecepcion"" >= @Fi AND o.""FechaRecepcion"" < @Ff + INTERVAL '1 day'
                    GROUP BY c.""NombreCategoria""
                    ORDER BY Total DESC";
        var result = await conn.QueryAsync<IngresoPorCategoriaDto>(sql, new { Fi = fi, Ff = ff });
        return result.ToList();
    }

    private async Task<ComparativaSemanalDto> GetComparativaSemanal(IDbConnection conn)
    {
        var hoy = DateTime.Today;
        var inicioSemanaActual = hoy.AddDays(-(int)hoy.DayOfWeek);
        var finSemanaActual = inicioSemanaActual.AddDays(6);
        var inicioSemanaAnterior = inicioSemanaActual.AddDays(-7);
        var finSemanaAnterior = inicioSemanaActual.AddDays(-1);

        var sqlActual = @"SELECT COALESCE(SUM(""Total""), 0) FROM ""Ordenes""
                          WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" <= @Ff + INTERVAL '1 day'";
        var sqlAnterior = @"SELECT COALESCE(SUM(""Total""), 0) FROM ""Ordenes""
                            WHERE ""FechaRecepcion"" >= @Fi AND ""FechaRecepcion"" <= @Ff + INTERVAL '1 day'";

        var actual = await conn.ExecuteScalarAsync<decimal>(sqlActual, new { Fi = inicioSemanaActual, Ff = finSemanaActual });
        var anterior = await conn.ExecuteScalarAsync<decimal>(sqlAnterior, new { Fi = inicioSemanaAnterior, Ff = finSemanaAnterior });

        return new ComparativaSemanalDto { SemanaActual = actual, SemanaAnterior = anterior };
    }
}
```

### Paso 2: Build

```bash
cd Backend
dotnet build LaundryManagement.sln 2>&1 | head -20
```

Esperado: sin errores de compilación.

### Paso 3: Registrar en DI

Registrar el servicio en `Infrastructure/DependencyInjection.cs` (es nuevo, hay que agregarlo):

```csharp
services.AddScoped<IDashboardService, DashboardService>();
```

### Paso 4: Commit

```bash
git add Backend/src/LaundryManagement.Infrastructure/Services/DashboardService.cs
git add Backend/src/LaundryManagement.Infrastructure/DependencyInjection.cs  # si se modificó
git commit -m "feat(backend): agregar DashboardService con queries Dapper"
```

---

## Task 4: Backend — DashboardController

**Files:**
- Create: `Backend/src/LaundryManagement.API/Controllers/DashboardController.cs`

### Paso 1: Crear el controlador

```csharp
using LaundryManagement.Application.DTOs;
using LaundryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        var fi = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var ff = fechaFin ?? DateTime.Today;

        var dto = await _dashboardService.GetDashboardAsync(fi, ff, ct);
        return Ok(dto);
    }
}
```

### Paso 2: Build

```bash
cd Backend
dotnet build LaundryManagement.sln 2>&1 | head -15
```

Esperado: sin errores.

### Paso 3: Commit

```bash
git add Backend/src/LaundryManagement.API/Controllers/DashboardController.cs
git commit -m "feat(backend): agregar DashboardController con endpoint GET /api/dashboard"
```

---

## Task 5: Frontend — Dependencias

### Paso 1: Instalar recharts

```bash
cd Frontend
npm install recharts
```

### Paso 2: Agregar componentes shadcn

```bash
cd Frontend
npx shadcn@latest add card badge separator skeleton
```

### Paso 3: Verificar installation

```bash
cd Frontend
npm run build 2>&1 | head -20
```

Esperado: sin errores de tipo.

### Paso 4: Commit

```bash
git add Frontend/package.json Frontend/package-lock.json
git add Frontend/src/shared/components/ui/
git commit -m "feat(frontend): agregar recharts y shadcn card/badge/separator"
```

---

## Task 6: Frontend — API y tipos

**Files:**
- Create: `Frontend/src/api/dashboard/index.ts`
- Create: `Frontend/src/features/dashboard/types/dashboard.ts`

### Paso 1: Crear tipos

```typescript
export interface IngresoPorMetodo {
  metodo: string;
  total: number;
}

export interface OrdenesPendientesPagar {
  cantidad: number;
  total: number;
}

export interface ClienteTop {
  nombre: string;
  ordenes: number;
}

export interface DashboardKPIs {
  ingresosTotales: number;
  ticketPromedio: number;
  totalDescuentos: number;
  ingresosPorMetodo: IngresoPorMetodo[];
  ordenesAtrasadas: number;
  ordenesPendientesPagar: OrdenesPendientesPagar;
  clientesNuevos: number;
  clienteTop: ClienteTop | null;
  totalCorteCaja: number;
  diferencias: number;
  transacciones: number;
}

export interface IngresoPorDia {
  fecha: string;
  ingresos: number;
  ordenes: number;
}

export interface OrdenesPorEstado {
  estado: string;
  cantidad: number;
}

export interface IngresoPorServicio {
  servicio: string;
  total: number;
}

export interface IngresoPorCategoria {
  categoria: string;
  total: number;
}

export interface ComparativaSemanal {
  semanaActual: number;
  semanaAnterior: number;
}

export interface DashboardCharts {
  ingresosPorDia: IngresoPorDia[];
  ordenesPorEstado: OrdenesPorEstado[];
  ingresosPorServicio: IngresoPorServicio[];
  ingresosPorCategoria: IngresoPorCategoria[];
  comparativaSemanal: ComparativaSemanal;
}

export interface DashboardDto {
  kpis: DashboardKPIs;
  charts: DashboardCharts;
}
```

### Paso 2: Crear API layer

```typescript
import api from '@/api/axiosConfig';
import type { DashboardDto } from '@/features/dashboard/types/dashboard';

export const getDashboard = async (fechaInicio: string, fechaFin: string): Promise<DashboardDto> => {
  const response = await api.get<DashboardDto>('/dashboard', {
    params: { fechaInicio, fechaFin },
  });
  return response.data;
};
```

### Paso 3: Commit

```bash
git add Frontend/src/features/dashboard/types/dashboard.ts
git add Frontend/src/api/dashboard/index.ts
git commit -m "feat(frontend): agregar API y tipos del dashboard"
```

---

## Task 7: Frontend — Zustand store

**Files:**
- Create: `Frontend/src/features/dashboard/stores/dashboardStore.ts`

### Paso 1: Crear el store

```typescript
import { create } from 'zustand';
import { format } from 'date-fns';
import { getDashboard } from '@/api/dashboard';
import type { DashboardDto } from '../types/dashboard';

interface DashboardState {
  kpis: DashboardDto['kpis'] | null;
  charts: DashboardDto['charts'] | null;
  fechaInicio: Date;
  fechaFin: Date;
  isLoading: boolean;
  error: string | null;
  setFechaRange: (inicio: Date, fin: Date) => void;
  fetchDashboard: () => Promise<void>;
}

const getDefaultRange = () => {
  const today = new Date();
  const firstOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
  return { fechaInicio: firstOfMonth, fechaFin: today };
};

export const useDashboardStore = create<DashboardState>((set, get) => ({
  ...getDefaultRange(),
  kpis: null,
  charts: null,
  isLoading: false,
  error: null,

  setFechaRange: (inicio, fin) => {
    set({ fechaInicio: inicio, fechaFin: fin });
    get().fetchDashboard();
  },

  fetchDashboard: async () => {
    set({ isLoading: true, error: null });
    try {
      const { fechaInicio, fechaFin } = get();
      const data = await getDashboard(
        format(fechaInicio, 'yyyy-MM-dd'),
        format(fechaFin, 'yyyy-MM-dd')
      );
      set({ kpis: data.kpis, charts: data.charts, isLoading: false });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Error al cargar dashboard';
      set({ error: message, isLoading: false });
    }
  },
}));
```

### Paso 2: Commit

```bash
git add Frontend/src/features/dashboard/stores/dashboardStore.ts
git commit -m "feat(frontend): agregar dashboardStore con Zustand"
```

---

## Task 8: Frontend — DashboardKPICard

**Files:**
- Create: `Frontend/src/features/dashboard/components/DashboardKPICard.tsx`

### Paso 1: Crear el componente

```tsx
import { type LucideIcon } from 'lucide-react';
import { Card, CardContent } from '@/shared/components/ui/card';
import { TrendingDown, TrendingUp, Minus } from 'lucide-react';

interface DashboardKPICardProps {
  title: string;
  value: string;
  subtitle?: string;
  icon: LucideIcon;
  trend?: 'up' | 'down' | 'neutral';
}

export function DashboardKPICard({ title, value, subtitle, icon: Icon, trend }: DashboardKPICardProps) {
  const TrendIcon = trend === 'up' ? TrendingUp : trend === 'down' ? TrendingDown : Minus;
  const trendColor = trend === 'up'
    ? 'text-emerald-600'
    : trend === 'down' ? 'text-rose-600' : 'text-zinc-400';

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-4">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <p className="text-xs text-muted-foreground font-medium uppercase tracking-wide">
              {title}
            </p>
            <p className="text-2xl font-bold text-zinc-900">{value}</p>
            {subtitle && (
              <p className={`text-xs flex items-center gap-1 ${trendColor}`}>
                <TrendIcon className="h-3 w-3" />
                {subtitle}
              </p>
            )}
          </div>
          <div className="p-2 bg-primary/10 rounded-lg">
            <Icon className="h-5 w-5 text-primary" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
```

### Paso 2: Commit

```bash
git add Frontend/src/features/dashboard/components/DashboardKPICard.tsx
git commit -m "feat(frontend): agregar DashboardKPICard"
```

---

## Task 9: Frontend — DashboardCardsGrid

**Files:**
- Create: `Frontend/src/features/dashboard/components/DashboardCardsGrid.tsx`

### Paso 1: Crear el grid de cards

```tsx
import {
  DollarSign,
  Percent,
  CreditCard,
  Clock,
  AlertCircle,
  Users,
  User,
  Banknote,
  Scale,
  Receipt
} from 'lucide-react';
import { DashboardKPICard } from './DashboardKPICard';
import { useDashboardStore } from '../stores/dashboardStore';
import { Skeleton } from '@/shared/components/ui/skeleton';

const formatMoney = (n: number) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(n);

export function DashboardCardsGrid() {
  const { kpis, isLoading } = useDashboardStore();

  if (isLoading || !kpis) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {Array.from({ length: 9 }).map((_, i) => (
          <Skeleton key={i} className="h-28" />
        ))}
      </div>
    );
  }

  const ordenesPendientes = kpis.ordenesPendientesPagar;
  const comparativaIngresos = kpis.clienteTop;

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      {/* Financieras */}
      <DashboardKPICard
        title="Ingresos Totales"
        value={formatMoney(kpis.ingresosTotales)}
        icon={DollarSign}
      />
      <DashboardKPICard
        title="Ticket Promedio"
        value={formatMoney(kpis.ticketPromedio)}
        icon={CreditCard}
      />
      <DashboardKPICard
        title="Descuentos"
        value={formatMoney(kpis.totalDescuentos)}
        icon={Percent}
      />
      {/* Operacionales */}
      <DashboardKPICard
        title="Órdenes Atrasadas"
        value={String(kpis.ordenesAtrasadas)}
        icon={Clock}
        trend={kpis.ordenesAtrasadas > 0 ? 'down' : 'up'}
      />
      <DashboardKPICard
        title="Pendientes por Pagar"
        value={`${ordenesPendientes.cantidad} órdenes`}
        subtitle={formatMoney(ordenesPendientes.total)}
        icon={AlertCircle}
        trend={ordenesPendientes.cantidad > 0 ? 'down' : 'neutral'}
      />
      {/* Clientes */}
      <DashboardKPICard
        title="Clientes Nuevos"
        value={String(kpis.clientesNuevos)}
        icon={Users}
      />
      {kpis.clienteTop && (
        <DashboardKPICard
          title="Cliente Top"
          value={kpis.clienteTop.nombre}
          subtitle={`${kpis.clienteTop.ordenes} órdenes`}
          icon={User}
        />
      )}
      {/* Caja */}
      <DashboardKPICard
        title="Total Corte Caja"
        value={formatMoney(kpis.totalCorteCaja)}
        icon={Banknote}
      />
      <DashboardKPICard
        title="Diferencias"
        value={String(kpis.diferencias)}
        icon={Scale}
        trend={kpis.diferencias > 0 ? 'down' : 'up'}
      />
      <DashboardKPICard
        title="Transacciones"
        value={String(kpis.transacciones)}
        icon={Receipt}
      />
    </div>
  );
}
```

### Paso 2: Commit

```bash
git add Frontend/src/features/dashboard/components/DashboardCardsGrid.tsx
git commit -m "feat(frontend): agregar DashboardCardsGrid con todas las KPI cards"
```

---

## Task 10: Frontend — Gráficos (6 componentes)

**Files:**
- Create: `Frontend/src/features/dashboard/components/RevenueTimelineChart.tsx`
- Create: `Frontend/src/features/dashboard/components/OrdersByStatusChart.tsx`
- Create: `Frontend/src/features/dashboard/components/RevenueByMethodChart.tsx`
- Create: `Frontend/src/features/dashboard/components/RevenueByServiceChart.tsx`
- Create: `Frontend/src/features/dashboard/components/RevenueByCategoryChart.tsx`
- Create: `Frontend/src/features/dashboard/components/WeeklyComparisonChart.tsx`

### Paso 1: RevenueTimelineChart

```tsx
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { format, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

const formatMoney = (v: number) => `${(v / 1000).toFixed(0)}k`;

export function RevenueTimelineChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  const data = charts.ingresosPorDia.map((d) => ({
    ...d,
    fecha: format(parseISO(d.fecha), 'dd MMM', { locale: es }),
  }));

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos y Órdenes por Día</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <LineChart data={data} margin={{ top: 5, right: 10, left: 0, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="fecha" tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <YAxis yAxisId="left" tickFormatter={formatMoney} tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <Tooltip
              formatter={(value: number, name: string) => [
                name === 'ingresos' ? `$${value.toLocaleString('es-MX')}` : String(value),
                name === 'ingresos' ? 'Ingresos' : 'Órdenes',
              ]}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
            <Line yAxisId="left" type="monotone" dataKey="ingresos" stroke="#6366f1" strokeWidth={2} dot={false} name="Ingresos" />
            <Line yAxisId="right" type="monotone" dataKey="ordenes" stroke="#f59e0b" strokeWidth={2} dot={false} name="Órdenes" />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
```

### Paso 2: OrdersByStatusChart

```tsx
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

const STATUS_COLORS: Record<string, string> = {
  'Recibida': '#3b82f6',
  'En Proceso': '#f59e0b',
  'Lista para Entregar': '#8b5cf6',
  'Entregada': '#10b981',
  'Cancelada': '#ef4444',
};

export function OrdersByStatusChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Órdenes por Estado</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={charts.ordenesPorEstado} layout="vertical" margin={{ top: 5, right: 30, left: 0, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis type="number" tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <YAxis dataKey="estado" type="category" tick={{ fontSize: 11 }} width={100} className="fill-zinc-500" />
            <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8 }} />
            <Bar dataKey="cantidad" radius={[0, 4, 4, 0]}>
              {charts.ordenesPorEstado.map((entry) => (
                <Cell key={entry.estado} fill={STATUS_COLORS[entry.estado] ?? '#6366f1'} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
```

### Paso 3: RevenueByMethodChart

```tsx
import { PieChart, Pie, Cell, Legend, Tooltip, ResponsiveContainer } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

const COLORS = ['#10b981', '#6366f1', '#f59e0b', '#ef4444', '#8b5cf6'];

const formatMoney = (v: number) => `$${v.toLocaleString('es-MX')}`;

export function RevenueByMethodChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos por Método de Pago</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <PieChart>
            <Pie
              data={charts.ingresosPorMetodo}
              dataKey="total"
              nameKey="metodo"
              cx="50%"
              cy="50%"
              innerRadius={60}
              outerRadius={90}
              paddingAngle={3}
            >
              {charts.ingresosPorMetodo.map((_, i) => (
                <Cell key={i} fill={COLORS[i % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip formatter={(v: number) => formatMoney(v)} contentStyle={{ fontSize: 12, borderRadius: 8 }} />
            <Legend wrapperStyle={{ fontSize: 12 }} />
          </PieChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
```

### Paso 4: RevenueByServiceChart

```tsx
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

export function RevenueByServiceChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos por Servicio</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={charts.ingresosPorServicio.slice(0, 10)} margin={{ top: 5, right: 10, left: 0, bottom: 50 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="servicio" tick={{ fontSize: 10 }} angle={-35} textAnchor="end" className="fill-zinc-500" />
            <YAxis tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <Tooltip
              formatter={(v: number) => [`$${v.toLocaleString('es-MX')}`, 'Ingresos']}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Bar dataKey="total" fill="#6366f1" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
```

### Paso 5: RevenueByCategoryChart

```tsx
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

export function RevenueByCategoryChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos por Categoría</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={charts.ingresosPorCategoria} margin={{ top: 5, right: 10, left: 0, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="categoria" tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <YAxis tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <Tooltip
              formatter={(v: number) => [`$${v.toLocaleString('es-MX')}`, 'Ingresos']}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Bar dataKey="total" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
```

### Paso 6: WeeklyComparisonChart

```tsx
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell, LabelList } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

export function WeeklyComparisonChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  const { semanaActual, semanaAnterior } = charts.comparativaSemanal;
  const diff = semanaAnterior > 0 ? ((semanaActual - semanaAnterior) / semanaAnterior) * 100 : 0;
  const diffLabel = `${diff >= 0 ? '+' : ''}${diff.toFixed(1)}%`;

  const data = [
    { label: 'Semana Anterior', value: semanaAnterior },
    { label: 'Semana Actual', value: semanaActual },
  ];

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">
          Comparativa Semanal — {diffLabel}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={data} margin={{ top: 20, right: 20, left: 0, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="label" tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <YAxis tick={{ fontSize: 11 }} className="fill-zinc-500" />
            <Tooltip
              formatter={(v: number) => [`$${v.toLocaleString('es-MX')}`, 'Ingresos']}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Bar dataKey="value" radius={[4, 4, 0, 0]}>
              <LabelList dataKey="value" formatter={(v: number) => `$${(v / 1000).toFixed(0)}k`} position="top" style={{ fontSize: 11, fill: '#71717a' }} />
              <Cell fill="#f59e0b" />
              <Cell fill="#10b981" />
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
```

### Paso 7: Commit

```bash
git add Frontend/src/features/dashboard/components/RevenueTimelineChart.tsx
git add Frontend/src/features/dashboard/components/OrdersByStatusChart.tsx
git add Frontend/src/features/dashboard/components/RevenueByMethodChart.tsx
git add Frontend/src/features/dashboard/components/RevenueByServiceChart.tsx
git add Frontend/src/features/dashboard/components/RevenueByCategoryChart.tsx
git add Frontend/src/features/dashboard/components/WeeklyComparisonChart.tsx
git commit -m "feat(frontend): agregar 6 componentes de gráficos con Recharts"
```

---

## Task 11: Frontend — Dashboard page

**Files:**
- Modify: `Frontend/src/features/dashboard/pages/Dashboard.tsx`

### Paso 1: Reemplazar el contenido del Dashboard

```tsx
import { useEffect } from 'react';
import { useDashboardStore } from '../stores/dashboardStore';
import { DashboardCardsGrid } from '../components/DashboardCardsGrid';
import { RevenueTimelineChart } from '../components/RevenueTimelineChart';
import { OrdersByStatusChart } from '../components/OrdersByStatusChart';
import { RevenueByMethodChart } from '../components/RevenueByMethodChart';
import { RevenueByServiceChart } from '../components/RevenueByServiceChart';
import { RevenueByCategoryChart } from '../components/RevenueByCategoryChart';
import { WeeklyComparisonChart } from '../components/WeeklyComparisonChart';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Separator } from '@/shared/components/ui/separator';
import { AlertCircle } from 'lucide-react';

export function Dashboard() {
  const { fetchDashboard, isLoading, error, fechaInicio, fechaFin, setFechaRange } = useDashboardStore();

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  if (error) {
    return (
      <div className="mx-auto max-w-4xl mt-8">
        <Card className="border-rose-200 bg-rose-50">
          <CardContent className="flex items-center gap-3 p-4">
            <AlertCircle className="h-5 w-5 text-rose-600" />
            <p className="text-sm text-rose-700">{error}</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header con filtro de fechas */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-zinc-900">Dashboard</h1>
        <div className="flex items-center gap-2">
          <input
            type="date"
            value={fechaInicio.toISOString().split('T')[0]}
            onChange={(e) => setFechaRange(new Date(e.target.value), fechaFin)}
            className="border rounded px-3 py-1.5 text-sm"
          />
          <span className="text-zinc-400">—</span>
          <input
            type="date"
            value={fechaFin.toISOString().split('T')[0]}
            onChange={(e) => setFechaRange(fechaInicio, new Date(e.target.value))}
            className="border rounded px-3 py-1.5 text-sm"
          />
        </div>
      </div>

      {/* KPI Cards */}
      <DashboardCardsGrid />

      <Separator />

      {/* Gráficos de línea + barras estado */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RevenueTimelineChart />
        <OrdersByStatusChart />
      </div>

      {/* Gráfico de método de pago */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RevenueByMethodChart />
        <WeeklyComparisonChart />
      </div>

      {/* Gráficos por servicio y categoría */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RevenueByServiceChart />
        <RevenueByCategoryChart />
      </div>
    </div>
  );
}
```

### Nota sobre filtro de fechas

Se usan dos `<input type="date">` nativos para evitar dependencias adicionales. Si más adelante se quiere un `DateRangePicker` con popover/calendario, se puede agregar como mejora con `npx shadcn@latest add calendar popover` y construir un componente en `shared/components/ui/date-range-picker.tsx`.

### Paso 2: Build

```bash
cd Frontend
npm run build 2>&1 | head -50
```

Esperado: sin errores de TypeScript.

### Paso 3: Commit

```bash
git add Frontend/src/features/dashboard/pages/Dashboard.tsx
git commit -m "feat(frontend): integrar Dashboard completo con cards, gráficos y filtro"
```

---

## Task 12: Prueba manual

### Verificar que el endpoint responde

1. Levantar backend: `cd Backend/src/LaundryManagement.API && dotnet run`
2. Abrir Swagger: `https://localhost:7037`
3. Autenticarse como admin
4. Ir a `GET /api/Dashboard` y ejecutar con rango de fechas
5. Verificar que devuelve JSON con `kpis` y `charts`

### Verificar frontend

1. Levantar frontend: `cd Frontend && npm run dev`
2. Ir a Dashboard (logueado como admin)
3. Verificar que las cards cargan con datos
4. Cambiar el rango de fechas y verificar que se actualizan
5. Verificar que los gráficos renderizan sin errores

### Commit final

```bash
git add -p  # solo ajustes
git commit -m "fix: ajustes post prueba manual dashboard"
```

---

## Notas de implementación

- Los queries SQL asumen PostgreSQL con INTERVAL syntax. Si la BD es SQL Server, cambiar `+ INTERVAL '1 day'` por `+ 1`.
- El endpoint requiere rol `admin` via `[Authorize(Roles = "admin")]`. Si el JWT no incluye el rol, fallará con 403.
- Recharts `ResponsiveContainer` requiere altura explícita — no usar `100%` sin definir la altura del contenedor padre.
- `date-fns/locale/es` se importa para formatear fechas en español.
- Si la API de Swagger no muestra el endpoint Dashboard, verificar que el controller se registra en `Program.cs`.
