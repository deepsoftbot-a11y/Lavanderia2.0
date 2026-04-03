# Historial de Órdenes — Plan de Implementación

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Reemplazar `OrdersList.tsx` con historial paginado (20/página), filtros de fecha/estado/pago, y exportación a Excel y PDF.

**Architecture:** Paginación en backend con EF Core (`Skip`/`Take` + `CountAsync`). Los filtros de estado de pago usan subquery EF Core contra la tabla `Pagos`. La exportación usa Dapper + ClosedXML/QuestPDF en `ReporteService`. El frontend mantiene la paginación y filtros activos en `ordersStore`.

**Tech Stack:** .NET 8 + EF Core 8 + ClosedXML 0.105 + QuestPDF 2026.2.4 | React 19 + TypeScript + Zustand/Immer + Axios + Shadcn/ui + TailwindCSS

---

## Contexto del código existente

Antes de tocar cualquier archivo, leer:
- `Backend/src/LaundryManagement.Domain/Repositories/IOrderRepository.cs` — interfaz actual
- `Backend/src/LaundryManagement.Infrastructure/Repositories/OrderRepositoryPure.cs` — implementación EF Core actual (líneas 217-262)
- `Backend/src/LaundryManagement.Application/Queries/Orders/GetOrdersQuery.cs` — record actual
- `Backend/src/LaundryManagement.Application/Queries/Orders/GetOrdersQueryHandler.cs` — handler actual
- `Backend/src/LaundryManagement.Application/Queries/Orders/SearchOrdersQueryHandler.cs` — también usa GetAllAsync
- `Backend/src/LaundryManagement.API/Controllers/OrdersController.cs` — endpoint GET /api/orders
- `Frontend/src/api/orders/ordersService.api.ts` — función getOrders actual
- `Frontend/src/features/orders/stores/ordersStore.ts` — estado actual
- `Frontend/src/api/orderStatuses/mockData.ts` — IDs: 1=Recibido, 2=Listo, 3=Entregado, 4=Cancelado

---

## Task 1: Crear `PagedResult<T>` en Application

**Files:**
- Create: `Backend/src/LaundryManagement.Application/Common/PagedResult.cs`

**Step 1: Crear el archivo**

```csharp
namespace LaundryManagement.Application.Common;

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize
);
```

**Step 2: Verificar que compila**

```bash
cd Backend
dotnet build src/LaundryManagement.Application/LaundryManagement.Application.csproj
```
Expected: Build succeeded, 0 errors.

**Step 3: Commit**

```bash
git add Backend/src/LaundryManagement.Application/Common/PagedResult.cs
git commit -m "feat: agregar PagedResult<T> en Application/Common"
```

---

## Task 2: Actualizar `IOrderRepository.GetAllAsync` y su implementación

**Files:**
- Modify: `Backend/src/LaundryManagement.Domain/Repositories/IOrderRepository.cs`
- Modify: `Backend/src/LaundryManagement.Infrastructure/Repositories/OrderRepositoryPure.cs`

**Step 1: Actualizar la interfaz `IOrderRepository`**

Reemplazar la firma de `GetAllAsync` (líneas 46-53) por:

```csharp
Task<(IEnumerable<OrderPure> Items, int TotalCount)> GetAllAsync(
    string? search = null,
    int? clientId = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    int[]? statusIds = null,
    string[]? paymentStatuses = null,
    string sortBy = "createdAt",
    string sortOrder = "desc",
    int page = 1,
    int pageSize = int.MaxValue,
    CancellationToken cancellationToken = default);
```

> Nota: `pageSize = int.MaxValue` como default mantiene compatibilidad con `SearchOrdersQueryHandler` que llama sin paginación.

**Step 2: Actualizar `OrderRepositoryPure.GetAllAsync`**

Reemplazar la implementación completa del método (desde `public async Task<IEnumerable<OrderPure>> GetAllAsync(` hasta el cierre `}`):

```csharp
public async Task<(IEnumerable<OrderPure> Items, int TotalCount)> GetAllAsync(
    string? search = null,
    int? clientId = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    int[]? statusIds = null,
    string[]? paymentStatuses = null,
    string sortBy = "createdAt",
    string sortOrder = "desc",
    int page = 1,
    int pageSize = int.MaxValue,
    CancellationToken cancellationToken = default)
{
    var query = _context.Ordenes
        .Include(o => o.OrdenesDetalles)
        .Include(o => o.OrdenesDescuentos)
        .Include(o => o.Cliente)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchLower = search.ToLower();
        query = query.Where(o =>
            o.FolioOrden.ToLower().Contains(searchLower) ||
            o.Cliente.NombreCompleto.ToLower().Contains(searchLower) ||
            o.Cliente.Telefono.Contains(search));
    }

    if (clientId.HasValue)
        query = query.Where(o => o.ClienteId == clientId.Value);

    if (startDate.HasValue)
        query = query.Where(o => o.FechaRecepcion >= startDate.Value);

    if (endDate.HasValue)
        query = query.Where(o => o.FechaRecepcion <= endDate.Value.AddDays(1).AddTicks(-1));

    if (statusIds != null && statusIds.Length > 0)
        query = query.Where(o => statusIds.Contains(o.EstadoOrdenId));

    if (paymentStatuses != null && paymentStatuses.Length > 0)
    {
        var needPaid    = paymentStatuses.Contains("paid");
        var needPartial = paymentStatuses.Contains("partial");
        var needPending = paymentStatuses.Contains("pending");

        query = query.Where(o =>
            (needPaid && o.Pagos.Where(p => p.CanceladoEn == null).Sum(p => (decimal?)p.MontoPago) >= o.Total && o.Total > 0) ||
            (needPartial && o.Pagos.Where(p => p.CanceladoEn == null).Sum(p => (decimal?)p.MontoPago) > 0 &&
                            o.Pagos.Where(p => p.CanceladoEn == null).Sum(p => (decimal?)p.MontoPago) < o.Total) ||
            (needPending && !(o.Pagos.Where(p => p.CanceladoEn == null).Sum(p => (decimal?)p.MontoPago) > 0))
        );
    }

    query = (sortBy.ToLower(), sortOrder.ToLower()) switch
    {
        ("folioorden", "asc")  => query.OrderBy(o => o.FolioOrden),
        ("folioorden", _)      => query.OrderByDescending(o => o.FolioOrden),
        ("total", "asc")       => query.OrderBy(o => o.Total),
        ("total", _)           => query.OrderByDescending(o => o.Total),
        (_, "asc")             => query.OrderBy(o => o.FechaRecepcion),
        _                      => query.OrderByDescending(o => o.FechaRecepcion)
    };

    var totalCount = await query.CountAsync(cancellationToken);

    var offset = (page - 1) * pageSize;
    IQueryable<Ordene> paged = pageSize == int.MaxValue
        ? query
        : query.Skip(offset).Take(pageSize);

    var entities = await paged.ToListAsync(cancellationToken);
    return (entities.Select(OrderMapper.ToDomain).ToList(), totalCount);
}
```

> Nota: El `using` para `Ordene` en el tipo del `IQueryable` requiere que ya esté en scope. Como el archivo ya usa `LaundryManagement.Infrastructure.Persistence.Entities`, no es necesario agregar using.

**Step 3: Verificar que compila**

```bash
cd Backend
dotnet build LaundryManagement.sln
```
Expected: errores en `GetOrdersQueryHandler` y `SearchOrdersQueryHandler` porque cambia el tipo de retorno. Eso es esperado y se corrige en Task 3 y Task 4.

**Step 4: Commit parcial (solo interfaz + repositorio)**

```bash
git add Backend/src/LaundryManagement.Domain/Repositories/IOrderRepository.cs
git add Backend/src/LaundryManagement.Infrastructure/Repositories/OrderRepositoryPure.cs
git commit -m "feat: GetAllAsync con paginación y filtros statusIds/paymentStatuses"
```

---

## Task 3: Actualizar `GetOrdersQuery` y `GetOrdersQueryHandler`

**Files:**
- Modify: `Backend/src/LaundryManagement.Application/Queries/Orders/GetOrdersQuery.cs`
- Modify: `Backend/src/LaundryManagement.Application/Queries/Orders/GetOrdersQueryHandler.cs`

**Step 1: Actualizar `GetOrdersQuery`**

Reemplazar el contenido completo:

```csharp
using LaundryManagement.Application.Common;
using LaundryManagement.Application.Queries.Orders;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public record GetOrdersQuery(
    string? Search,
    int? ClientId,
    DateTime? StartDate,
    DateTime? EndDate,
    int[]? StatusIds,
    string[]? PaymentStatuses,
    string SortBy = "createdAt",
    string SortOrder = "desc",
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderResponseDto>>;
```

**Step 2: Actualizar `GetOrdersQueryHandler`**

Cambiar la firma del handler y el método `Handle`:

1. Cambiar la declaración del handler:
```csharp
public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderResponseDto>>
```

2. Añadir el using al inicio del archivo:
```csharp
using LaundryManagement.Application.Common;
```

3. Reemplazar el método `Handle` completo:
```csharp
public async Task<PagedResult<OrderResponseDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
{
    var (orders, totalCount) = await _orderRepository.GetAllAsync(
        search: query.Search,
        clientId: query.ClientId,
        startDate: query.StartDate,
        endDate: query.EndDate,
        statusIds: query.StatusIds,
        paymentStatuses: query.PaymentStatuses,
        sortBy: query.SortBy,
        sortOrder: query.SortOrder,
        page: query.Page,
        pageSize: query.PageSize,
        cancellationToken: cancellationToken
    );

    var ordersList = orders.ToList();

    if (!ordersList.Any())
        return new PagedResult<OrderResponseDto>(new List<OrderResponseDto>(), totalCount, query.Page, query.PageSize);

    var orderIds = ordersList.Select(o => o.Id.Value).ToList();
    var allItems = ordersList.SelectMany(o => o.LineItems).ToList();
    var serviceIds = allItems.Select(i => i.ServiceId).Distinct().ToList();
    var servicioPrendaIds = allItems.Where(i => i.ServiceGarmentId.HasValue)
        .Select(i => i.ServiceGarmentId!.Value).Distinct().ToList();

    var (amountsPaid, paymentsByOrder, clientsDict, servicesDict, garmentTypesDict) =
        await FetchRelatedDataAsync(
            orderIds, ordersList.Select(o => o.ClientId).Distinct(),
            serviceIds, servicioPrendaIds, cancellationToken);

    var data = ordersList
        .Select(order => BuildDto(order, amountsPaid, paymentsByOrder, clientsDict, servicesDict, garmentTypesDict))
        .ToList();

    return new PagedResult<OrderResponseDto>(data, totalCount, query.Page, query.PageSize);
}
```

**Step 3: Compilar**

```bash
cd Backend
dotnet build src/LaundryManagement.Application/LaundryManagement.Application.csproj
```
Expected: aún puede haber error en `OrdersController` porque recibe `List<OrderResponseDto>`. Se corrige en Task 5.

**Step 4: Commit**

```bash
git add Backend/src/LaundryManagement.Application/Queries/Orders/GetOrdersQuery.cs
git add Backend/src/LaundryManagement.Application/Queries/Orders/GetOrdersQueryHandler.cs
git commit -m "feat: GetOrdersQuery con paginación y filtros de estado"
```

---

## Task 4: Corregir `SearchOrdersQueryHandler`

**Files:**
- Modify: `Backend/src/LaundryManagement.Application/Queries/Orders/SearchOrdersQueryHandler.cs`

**Step 1: Actualizar la llamada a `GetAllAsync`**

Reemplazar:
```csharp
var orders = (await _orderRepository.GetAllAsync(
    search: query.Query,
    clientId: null,
    startDate: null,
    endDate: null,
    cancellationToken: cancellationToken
)).ToList();
```

Por:
```csharp
var (ordersEnumerable, _) = await _orderRepository.GetAllAsync(
    search: query.Query,
    cancellationToken: cancellationToken
);
var orders = ordersEnumerable.ToList();
```

**Step 2: Compilar la solución completa**

```bash
cd Backend
dotnet build LaundryManagement.sln
```
Expected: Build succeeded, 0 errors (excepto posible error en `OrdersController` — se corrige en Task 5).

**Step 3: Commit**

```bash
git add Backend/src/LaundryManagement.Application/Queries/Orders/SearchOrdersQueryHandler.cs
git commit -m "fix: actualizar SearchOrdersQueryHandler para nueva firma de GetAllAsync"
```

---

## Task 5: Actualizar `OrdersController.GetOrders`

**Files:**
- Modify: `Backend/src/LaundryManagement.API/Controllers/OrdersController.cs`

**Step 1: Actualizar el endpoint `GetOrders`**

Reemplazar el método `GetOrders` completo:

```csharp
/// <summary>
/// Lista órdenes con filtros y paginación
/// </summary>
[HttpGet]
[ProducesResponseType(typeof(PagedResult<OrderResponseDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetOrders(
    [FromQuery] string? search,
    [FromQuery] int? clientId,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] int[]? statusIds,
    [FromQuery] string[]? paymentStatuses,
    [FromQuery] string sortBy = "createdAt",
    [FromQuery] string sortOrder = "desc",
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var query = new GetOrdersQuery(
        search, clientId, startDate, endDate,
        statusIds, paymentStatuses,
        sortBy, sortOrder, page, pageSize);
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**Step 2: Agregar el using en la parte superior del archivo**

```csharp
using LaundryManagement.Application.Common;
```

**Step 3: Compilar la solución completa**

```bash
cd Backend
dotnet build LaundryManagement.sln
```
Expected: Build succeeded, 0 errors.

**Step 4: Probar manualmente**

Con el backend corriendo:
```
GET https://localhost:7037/api/orders?page=1&pageSize=20
```
Expected: `{ "data": [...], "totalCount": N, "page": 1, "pageSize": 20 }`

```
GET https://localhost:7037/api/orders?statusIds=1&statusIds=2&page=1&pageSize=20
```
Expected: solo órdenes con EstadoOrdenId 1 o 2.

**Step 5: Commit**

```bash
git add Backend/src/LaundryManagement.API/Controllers/OrdersController.cs
git commit -m "feat: GetOrders con paginación y filtros statusIds/paymentStatuses"
```

---

## Task 6: Endpoint de exportación Excel/PDF

**Files:**
- Modify: `Backend/src/LaundryManagement.Application/Interfaces/IReporteService.cs`
- Modify: `Backend/src/LaundryManagement.Infrastructure/Services/ReporteService.cs`
- Modify: `Backend/src/LaundryManagement.API/Controllers/ReportesController.cs`

### Step 1: Crear DTO de fila de exportación

Crear archivo `Backend/src/LaundryManagement.Application/DTOs/Reportes/OrdenExportRow.cs`:

```csharp
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
```

### Step 2: Extender `IReporteService`

Agregar método al final de la interfaz:

```csharp
Task<byte[]> ExportOrdenesExcelAsync(
    DateTime? startDate,
    DateTime? endDate,
    int[]? statusIds,
    string[]? paymentStatuses);

Task<byte[]> ExportOrdenesPdfAsync(
    DateTime? startDate,
    DateTime? endDate,
    int[]? statusIds,
    string[]? paymentStatuses);
```

### Step 3: Agregar query privada en `ReporteService`

En `ReporteService.cs`, agregar método privado para obtener las filas:

```csharp
private async Task<List<OrdenExportRow>> GetOrdenExportRowsAsync(
    DateTime? startDate,
    DateTime? endDate,
    int[]? statusIds,
    string[]? paymentStatuses)
{
    using var connection = _connectionFactory.CreateConnection();

    var conditions = new List<string> { "1=1" };
    var parameters = new DynamicParameters();

    if (startDate.HasValue)
    {
        conditions.Add(@"o.""FechaRecepcion"" >= @StartDate");
        parameters.Add("StartDate", startDate.Value.Date);
    }
    if (endDate.HasValue)
    {
        conditions.Add(@"o.""FechaRecepcion"" < @EndDate");
        parameters.Add("EndDate", endDate.Value.Date.AddDays(1));
    }
    if (statusIds != null && statusIds.Length > 0)
    {
        conditions.Add(@"o.""EstadoOrdenID"" = ANY(@StatusIds)");
        parameters.Add("StatusIds", statusIds);
    }

    var where = string.Join(" AND ", conditions);

    var sql = $"""
        SELECT
            o."FolioOrden"                                              AS Folio,
            c."NombreCompleto"                                          AS Cliente,
            o."FechaRecepcion"                                          AS FechaCreacion,
            o."FechaPrometida"                                          AS FechaPrometida,
            eo."NombreEstado"                                           AS EstadoOrden,
            COALESCE(SUM(p."MontoPago") FILTER (WHERE p."CanceladoEn" IS NULL), 0) AS Pagado,
            o."Subtotal"                                                AS Subtotal,
            o."Descuento"                                               AS Descuento,
            o."Total"                                                   AS Total
        FROM "Ordenes" o
        JOIN "Clientes" c ON c."ClienteID" = o."ClienteID"
        JOIN "EstadosOrden" eo ON eo."EstadoOrdenID" = o."EstadoOrdenID"
        LEFT JOIN "Pagos" p ON p."OrdenID" = o."OrdenID"
        WHERE {where}
        GROUP BY o."OrdenID", o."FolioOrden", c."NombreCompleto",
                 o."FechaRecepcion", o."FechaPrometida", eo."NombreEstado",
                 o."Subtotal", o."Descuento", o."Total"
        ORDER BY o."FechaRecepcion" DESC
        """;

    var rows = (await connection.QueryAsync<dynamic>(sql, parameters)).ToList();

    // Filtrar por payment status en memoria (ya que es calculado)
    var result = rows.Select(r => {
        decimal pagado = (decimal)r.Pagado;
        decimal total  = (decimal)r.Total;
        string estPago = pagado >= total && total > 0 ? "paid"
                       : pagado > 0 ? "partial" : "pending";
        return new OrdenExportRow(
            Folio:          (string)r.Folio,
            Cliente:        (string)r.Cliente,
            FechaCreacion:  (DateTime)r.FechaCreacion,
            FechaPrometida: (DateTime)r.FechaPrometida,
            EstadoOrden:    (string)r.EstadoOrden,
            EstadoPago:     estPago == "paid" ? "Pagado" : estPago == "partial" ? "Parcial" : "Pendiente",
            Subtotal:       (decimal)r.Subtotal,
            Descuento:      (decimal)r.Descuento,
            Total:          total,
            Pagado:         pagado,
            Saldo:          total - pagado
        );
    }).ToList();

    if (paymentStatuses != null && paymentStatuses.Length > 0)
    {
        var statusMap = new Dictionary<string, string>
        {
            ["paid"]    = "Pagado",
            ["partial"] = "Parcial",
            ["pending"] = "Pendiente"
        };
        var labels = paymentStatuses
            .Where(s => statusMap.ContainsKey(s))
            .Select(s => statusMap[s])
            .ToHashSet();
        result = result.Where(r => labels.Contains(r.EstadoPago)).ToList();
    }

    return result;
}
```

### Step 4: Implementar `ExportOrdenesExcelAsync`

```csharp
public async Task<byte[]> ExportOrdenesExcelAsync(
    DateTime? startDate,
    DateTime? endDate,
    int[]? statusIds,
    string[]? paymentStatuses)
{
    var rows = await GetOrdenExportRowsAsync(startDate, endDate, statusIds, paymentStatuses);

    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Órdenes");

    // Headers
    string[] headers = { "Folio", "Cliente", "Fecha Creación", "Fecha Prometida",
                          "Estado Orden", "Estado Pago", "Subtotal", "Descuento",
                          "Total", "Pagado", "Saldo" };
    for (int i = 0; i < headers.Length; i++)
    {
        var cell = ws.Cell(1, i + 1);
        cell.Value = headers[i];
        cell.Style.Font.Bold = true;
        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 99, 235);
        cell.Style.Font.FontColor = XLColor.White;
    }

    // Data rows
    for (int i = 0; i < rows.Count; i++)
    {
        var r = rows[i];
        int row = i + 2;
        ws.Cell(row, 1).Value  = r.Folio;
        ws.Cell(row, 2).Value  = r.Cliente;
        ws.Cell(row, 3).Value  = r.FechaCreacion;
        ws.Cell(row, 4).Value  = r.FechaPrometida;
        ws.Cell(row, 5).Value  = r.EstadoOrden;
        ws.Cell(row, 6).Value  = r.EstadoPago;
        ws.Cell(row, 7).Value  = r.Subtotal;
        ws.Cell(row, 8).Value  = r.Descuento;
        ws.Cell(row, 9).Value  = r.Total;
        ws.Cell(row, 10).Value = r.Pagado;
        ws.Cell(row, 11).Value = r.Saldo;

        // Formato moneda para columnas 7-11
        for (int col = 7; col <= 11; col++)
            ws.Cell(row, col).Style.NumberFormat.Format = "$#,##0.00";
        // Formato fecha para columnas 3-4
        ws.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";
        ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";
    }

    ws.Columns().AdjustToContents();

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return stream.ToArray();
}
```

Agregar el using al inicio del archivo `ReporteService.cs`:
```csharp
using ClosedXML.Excel;
using LaundryManagement.Application.DTOs.Reportes;
```

### Step 5: Implementar `ExportOrdenesPdfAsync`

```csharp
public async Task<byte[]> ExportOrdenesPdfAsync(
    DateTime? startDate,
    DateTime? endDate,
    int[]? statusIds,
    string[]? paymentStatuses)
{
    var rows = await GetOrdenExportRowsAsync(startDate, endDate, statusIds, paymentStatuses);

    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1, Unit.Centimetre);

            page.Header().Text($"Historial de Órdenes — {DateTime.Today:dd/MM/yyyy}")
                .FontSize(14).Bold().AlignCenter();

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);  // Folio
                    cols.RelativeColumn(3);  // Cliente
                    cols.RelativeColumn(2);  // Fecha Creación
                    cols.RelativeColumn(2);  // Fecha Prometida
                    cols.RelativeColumn(2);  // Estado Orden
                    cols.RelativeColumn(2);  // Estado Pago
                    cols.RelativeColumn(1.5f); // Subtotal
                    cols.RelativeColumn(1.5f); // Descuento
                    cols.RelativeColumn(1.5f); // Total
                    cols.RelativeColumn(1.5f); // Pagado
                    cols.RelativeColumn(1.5f); // Saldo
                });

                // Header row
                static IContainer HeaderCell(IContainer c) =>
                    c.Background("#2563eb").Padding(4);

                string[] headers = { "Folio", "Cliente", "Creación", "Prometida",
                                     "Estado Orden", "Estado Pago", "Subtotal", "Descuento",
                                     "Total", "Pagado", "Saldo" };
                table.Header(header =>
                {
                    foreach (var h in headers)
                        header.Cell().Element(HeaderCell)
                            .Text(h).FontColor("#ffffff").FontSize(8).Bold();
                });

                // Data rows
                foreach (var r in rows)
                {
                    static IContainer DataCell(IContainer c) => c.BorderBottom(1).BorderColor("#e5e7eb").Padding(4);

                    table.Cell().Element(DataCell).Text(r.Folio).FontSize(7);
                    table.Cell().Element(DataCell).Text(r.Cliente).FontSize(7);
                    table.Cell().Element(DataCell).Text(r.FechaCreacion.ToString("dd/MM/yy")).FontSize(7);
                    table.Cell().Element(DataCell).Text(r.FechaPrometida.ToString("dd/MM/yy")).FontSize(7);
                    table.Cell().Element(DataCell).Text(r.EstadoOrden).FontSize(7);
                    table.Cell().Element(DataCell).Text(r.EstadoPago).FontSize(7);
                    table.Cell().Element(DataCell).Text($"${r.Subtotal:N2}").FontSize(7).AlignRight();
                    table.Cell().Element(DataCell).Text($"${r.Descuento:N2}").FontSize(7).AlignRight();
                    table.Cell().Element(DataCell).Text($"${r.Total:N2}").FontSize(7).Bold().AlignRight();
                    table.Cell().Element(DataCell).Text($"${r.Pagado:N2}").FontSize(7).AlignRight();
                    table.Cell().Element(DataCell).Text($"${r.Saldo:N2}").FontSize(7).AlignRight();
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Página ").FontSize(8);
                x.CurrentPageNumber().FontSize(8);
                x.Span(" de ").FontSize(8);
                x.TotalPages().FontSize(8);
            });
        });
    });

    return document.GeneratePdf();
}
```

Agregar usings:
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
```

### Step 6: Agregar endpoint en `ReportesController`

```csharp
/// <summary>
/// Exporta el historial de órdenes con filtros a Excel o PDF
/// </summary>
/// <param name="format">Formato de exportación: xlsx | pdf</param>
[HttpGet("ordenes/export")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ExportOrdenes(
    [FromQuery] string format,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] int[]? statusIds,
    [FromQuery] string[]? paymentStatuses)
{
    if (format != "xlsx" && format != "pdf")
        return BadRequest(new { message = "format debe ser 'xlsx' o 'pdf'" });

    if (format == "xlsx")
    {
        var bytes = await _reporteService.ExportOrdenesExcelAsync(startDate, endDate, statusIds, paymentStatuses);
        var fileName = $"ordenes-{DateTime.Today:yyyy-MM-dd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
    else
    {
        var bytes = await _reporteService.ExportOrdenesPdfAsync(startDate, endDate, statusIds, paymentStatuses);
        var fileName = $"ordenes-{DateTime.Today:yyyy-MM-dd}.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}
```

### Step 7: Compilar y probar

```bash
cd Backend
dotnet build LaundryManagement.sln
```
Expected: Build succeeded, 0 errors.

```bash
cd src/LaundryManagement.API && dotnet run
```
Probar en Swagger:
```
GET /api/reportes/ordenes/export?format=xlsx
```
Expected: descarga de archivo `.xlsx`.

### Step 8: Commit

```bash
git add Backend/src/LaundryManagement.Application/DTOs/Reportes/OrdenExportRow.cs
git add Backend/src/LaundryManagement.Application/Interfaces/IReporteService.cs
git add Backend/src/LaundryManagement.Infrastructure/Services/ReporteService.cs
git add Backend/src/LaundryManagement.API/Controllers/ReportesController.cs
git commit -m "feat: exportación de órdenes a Excel y PDF en ReportesController"
```

---

## Task 7: Actualizar tipos en `order.ts`

**Files:**
- Modify: `Frontend/src/features/orders/types/order.ts`

**Step 1: Agregar `PagedResult<T>` y `OrderHistoryFilters`**

Al final del archivo, agregar:

```typescript
export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface OrderHistoryFilters {
  startDate?: string;       // "2026-01-01"
  endDate?: string;         // "2026-12-31"
  statusIds?: number[];
  paymentStatuses?: PaymentStatus[];
}
```

**Step 2: Verificar que no hay errores de TypeScript**

```bash
cd Frontend
npm run build 2>&1 | head -30
```
Expected: sin errores de tipo.

**Step 3: Commit**

```bash
git add Frontend/src/features/orders/types/order.ts
git commit -m "feat: agregar PagedResult y OrderHistoryFilters en order.ts"
```

---

## Task 8: Actualizar API layer del frontend

**Files:**
- Modify: `Frontend/src/api/orders/ordersService.api.ts`

**Step 1: Actualizar `getOrders` y agregar `exportOrders`**

Reemplazar la función `getOrders` y agregar `exportOrders` al final:

```typescript
export async function getOrders(
  filters: OrderHistoryFilters = {},
  page = 1,
  pageSize = 20
): Promise<PagedResult<Order>> {
  try {
    const params: Record<string, unknown> = { ...filters, page, pageSize };
    const response = await api.get<PagedResult<Order>>('/orders', { params });
    return {
      ...response.data,
      data: response.data.data.map(withOrderStatus),
    };
  } catch (error) {
    console.error('Get orders API error:', error);
    throw new Error('Error al obtener órdenes desde el servidor');
  }
}

export async function exportOrders(
  format: 'xlsx' | 'pdf',
  filters: OrderHistoryFilters = {}
): Promise<void> {
  try {
    const params: Record<string, unknown> = { ...filters, format };
    const response = await api.get('/reportes/ordenes/export', {
      params,
      responseType: 'blob',
    });
    const ext = format === 'xlsx' ? 'xlsx' : 'pdf';
    const today = new Date().toISOString().slice(0, 10);
    const fileName = `ordenes-${today}.${ext}`;
    const url = URL.createObjectURL(response.data as Blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  } catch (error) {
    console.error('Export orders API error:', error);
    throw new Error('Error al generar el reporte');
  }
}
```

Agregar los imports al inicio del archivo:
```typescript
import type {
  Order,
  OrderSummary,
  PagedResult,
  OrderHistoryFilters,
  CreateOrderInput,
  UpdateOrderInput,
  OrderSearchFilters,
  UpdateOrderStatusInput,
} from '@/features/orders/types/order';
```

**Step 2: Compilar**

```bash
cd Frontend && npm run build 2>&1 | head -40
```
Expected: sin errores relacionados a `getOrders`.

**Step 3: Commit**

```bash
git add Frontend/src/api/orders/ordersService.api.ts
git commit -m "feat: getOrders paginado y exportOrders en API layer"
```

---

## Task 9: Actualizar `ordersStore.ts`

**Files:**
- Modify: `Frontend/src/features/orders/stores/ordersStore.ts`

**Step 1: Actualizar el store**

Reemplazar el contenido completo:

```typescript
import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';
import type {
  Order,
  OrderSummary,
  OrderHistoryFilters,
  PagedResult,
  OrderSearchFilters,
} from '@/features/orders/types/order';
import * as ordersApi from '@/api/orders';

interface OrdersPagination {
  page: number;
  pageSize: number;
  totalCount: number;
}

interface OrdersState {
  orders: Order[];
  selectedOrder: Order | null;
  isLoading: boolean;
  error: string | null;

  pagination: OrdersPagination;
  activeFilters: OrderHistoryFilters;

  // Search state
  searchResults: OrderSummary[];
  isSearching: boolean;
  searchError: string | null;

  // Actions
  fetchOrders: (filters?: OrderHistoryFilters, page?: number) => Promise<void>;
  fetchOrderById: (id: number) => Promise<void>;
  createOrder: (input: any) => Promise<Order | null>;
  updateOrder: (id: number, input: any) => Promise<Order | null>;
  deleteOrder: (id: number) => Promise<void>;
  setSelectedOrder: (order: Order | null) => void;
  clearError: () => void;
  clearFilters: () => void;

  // Search actions
  searchOrders: (filters: OrderSearchFilters) => Promise<void>;
  updateOrderStatus: (orderId: number, statusId: number) => Promise<Order | null>;
  refreshSelectedOrder: () => Promise<void>;
  clearSearchResults: () => void;
}

const DEFAULT_PAGINATION: OrdersPagination = { page: 1, pageSize: 20, totalCount: 0 };

export const useOrdersStore = create<OrdersState>()(
  immer((set, get) => ({
    orders: [],
    selectedOrder: null,
    isLoading: false,
    error: null,

    pagination: DEFAULT_PAGINATION,
    activeFilters: {},

    searchResults: [],
    isSearching: false,
    searchError: null,

    fetchOrders: async (filters = {}, page = 1) => {
      set({ isLoading: true, error: null });
      try {
        const result: PagedResult<Order> = await ordersApi.getOrders(filters, page);
        set({
          orders: result.data,
          pagination: { page: result.page, pageSize: result.pageSize, totalCount: result.totalCount },
          activeFilters: filters,
          isLoading: false,
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar órdenes';
        set({ error: message, isLoading: false });
      }
    },

    fetchOrderById: async (id) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.getOrderById(id);
        set({ selectedOrder: order, isLoading: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar orden';
        set({ error: message, isLoading: false });
      }
    },

    createOrder: async (input) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.createOrder(input);
        set((state) => {
          state.orders.unshift(order);
          state.pagination.totalCount += 1;
          state.isLoading = false;
        });
        return order;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear orden';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    updateOrder: async (id, input) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.updateOrder(id, input);
        set((state) => {
          const index = state.orders.findIndex((o) => o.id === id);
          if (index !== -1) state.orders[index] = order;
          if (state.selectedOrder?.id === id) state.selectedOrder = order;
          state.isLoading = false;
        });
        return order;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar orden';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    deleteOrder: async (id) => {
      set({ isLoading: true, error: null });
      try {
        await ordersApi.deleteOrder(id);
        set((state) => {
          state.orders = state.orders.filter((o) => o.id !== id);
          if (state.selectedOrder?.id === id) state.selectedOrder = null;
          state.pagination.totalCount = Math.max(0, state.pagination.totalCount - 1);
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar orden';
        set({ error: message, isLoading: false });
        throw error;
      }
    },

    setSelectedOrder: (order) => set({ selectedOrder: order }),
    clearError: () => set({ error: null }),
    clearFilters: () => {
      const { fetchOrders } = get();
      fetchOrders({}, 1);
    },

    searchOrders: async (filters) => {
      set({ isSearching: true, searchError: null });
      try {
        const results: OrderSummary[] = await ordersApi.searchOrders(filters);
        set({ searchResults: results, isSearching: false });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al buscar órdenes';
        set({ searchError: message, isSearching: false });
      }
    },

    updateOrderStatus: async (orderId, statusId) => {
      set({ isLoading: true, error: null });
      try {
        const order = await ordersApi.updateOrderStatus(orderId, { orderStatusId: statusId });
        set((state) => {
          const index = state.orders.findIndex((o) => o.id === orderId);
          if (index !== -1) state.orders[index] = order;
          const searchIndex = state.searchResults.findIndex((o) => o.id === orderId);
          if (searchIndex !== -1) state.searchResults[searchIndex] = order;
          state.isLoading = false;
        });
        return order;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar estado';
        set({ error: message, isLoading: false });
        return null;
      }
    },

    refreshSelectedOrder: async () => {
      const selectedOrder = get().selectedOrder;
      if (!selectedOrder) return;
      try {
        const order = await ordersApi.getOrderById(selectedOrder.id);
        set((state) => {
          state.selectedOrder = order;
          const searchIndex = state.searchResults.findIndex((o) => o.id === order.id);
          if (searchIndex !== -1) state.searchResults[searchIndex] = order;
        });
      } catch (error) {
        console.error('Error refreshing selected order:', error);
      }
    },

    clearSearchResults: () => set({ searchResults: [], searchError: null }),
  }))
);
```

**Step 2: Compilar**

```bash
cd Frontend && npm run build 2>&1 | head -40
```
Expected: sin errores de tipo.

**Step 3: Commit**

```bash
git add Frontend/src/features/orders/stores/ordersStore.ts
git commit -m "feat: ordersStore con paginación y filtros activos"
```

---

## Task 10: Crear `OrdersFiltersBar.tsx`

**Files:**
- Create: `Frontend/src/features/orders/components/ordersHistory/OrdersFiltersBar.tsx`

**Step 1: Crear el componente**

```tsx
import { useState } from 'react';
import { X } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { cn } from '@/shared/utils/cn';
import { mockOrderStatuses } from '@/api/orderStatuses';
import type { OrderHistoryFilters } from '@/features/orders/types/order';
import type { PaymentStatus } from '@/features/orders/types/payment';

interface OrdersFiltersBarProps {
  onFiltersChange: (filters: OrderHistoryFilters) => void;
}

const PAYMENT_STATUS_OPTIONS: { value: PaymentStatus; label: string }[] = [
  { value: 'paid',    label: 'Pagado'   },
  { value: 'partial', label: 'Parcial'  },
  { value: 'pending', label: 'Pendiente' },
];

export function OrdersFiltersBar({ onFiltersChange }: OrdersFiltersBarProps) {
  const [startDate, setStartDate]         = useState('');
  const [endDate, setEndDate]             = useState('');
  const [selectedStatusIds, setSelectedStatusIds] = useState<number[]>([]);
  const [selectedPayments, setSelectedPayments]   = useState<PaymentStatus[]>([]);
  const [dateError, setDateError]         = useState('');

  const hasFilters =
    startDate !== '' || endDate !== '' ||
    selectedStatusIds.length > 0 || selectedPayments.length > 0;

  function buildFilters(
    sd: string,
    ed: string,
    statuses: number[],
    payments: PaymentStatus[]
  ): OrderHistoryFilters {
    return {
      startDate:      sd || undefined,
      endDate:        ed || undefined,
      statusIds:      statuses.length > 0 ? statuses : undefined,
      paymentStatuses: payments.length > 0 ? payments : undefined,
    };
  }

  function handleStartDate(value: string) {
    setStartDate(value);
    if (endDate && value > endDate) {
      setDateError('La fecha de inicio no puede ser posterior a la fecha de fin');
      return;
    }
    setDateError('');
    onFiltersChange(buildFilters(value, endDate, selectedStatusIds, selectedPayments));
  }

  function handleEndDate(value: string) {
    setEndDate(value);
    if (startDate && value < startDate) {
      setDateError('La fecha de fin no puede ser anterior a la fecha de inicio');
      return;
    }
    setDateError('');
    onFiltersChange(buildFilters(startDate, value, selectedStatusIds, selectedPayments));
  }

  function handleStatusChange(id: number) {
    const next = selectedStatusIds.includes(id)
      ? selectedStatusIds.filter((s) => s !== id)
      : [...selectedStatusIds, id];
    setSelectedStatusIds(next);
    onFiltersChange(buildFilters(startDate, endDate, next, selectedPayments));
  }

  function handlePaymentToggle(status: PaymentStatus) {
    const next = selectedPayments.includes(status)
      ? selectedPayments.filter((s) => s !== status)
      : [...selectedPayments, status];
    setSelectedPayments(next);
    onFiltersChange(buildFilters(startDate, endDate, selectedStatusIds, next));
  }

  function handleClear() {
    setStartDate('');
    setEndDate('');
    setSelectedStatusIds([]);
    setSelectedPayments([]);
    setDateError('');
    onFiltersChange({});
  }

  return (
    <div className="border-b border-zinc-100 bg-zinc-50 px-6 py-4 space-y-3">
      <div className="flex flex-wrap gap-4 items-end">
        {/* Rango de fechas */}
        <div className="flex gap-2 items-end">
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500">Desde</Label>
            <Input
              type="date"
              value={startDate}
              onChange={(e) => handleStartDate(e.target.value)}
              className="h-8 text-xs w-36"
            />
          </div>
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500">Hasta</Label>
            <Input
              type="date"
              value={endDate}
              onChange={(e) => handleEndDate(e.target.value)}
              className="h-8 text-xs w-36"
            />
          </div>
        </div>

        {/* Estado de orden */}
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500">Estado de orden</Label>
          <div className="flex gap-1 flex-wrap">
            {mockOrderStatuses.map((status) => (
              <button
                key={status.id}
                type="button"
                onClick={() => handleStatusChange(status.id)}
                className={cn(
                  'px-2.5 py-1 rounded-full text-xs font-medium border transition-colors',
                  selectedStatusIds.includes(status.id)
                    ? 'bg-zinc-900 text-white border-zinc-900'
                    : 'bg-white text-zinc-600 border-zinc-200 hover:border-zinc-400'
                )}
              >
                {status.name}
              </button>
            ))}
          </div>
        </div>

        {/* Estado de pago */}
        <div className="space-y-1">
          <Label className="text-xs text-zinc-500">Estado de pago</Label>
          <div className="flex gap-1">
            {PAYMENT_STATUS_OPTIONS.map(({ value, label }) => (
              <button
                key={value}
                type="button"
                onClick={() => handlePaymentToggle(value)}
                className={cn(
                  'px-2.5 py-1 rounded-full text-xs font-medium border transition-colors',
                  selectedPayments.includes(value)
                    ? 'bg-zinc-900 text-white border-zinc-900'
                    : 'bg-white text-zinc-600 border-zinc-200 hover:border-zinc-400'
                )}
              >
                {label}
              </button>
            ))}
          </div>
        </div>

        {/* Limpiar */}
        {hasFilters && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleClear}
            className="h-8 text-xs text-zinc-400 hover:text-zinc-600"
          >
            <X className="h-3 w-3 mr-1" />
            Limpiar
          </Button>
        )}
      </div>

      {dateError && (
        <p className="text-xs text-red-500">{dateError}</p>
      )}
    </div>
  );
}
```

> Nota: Este componente requiere que `Label` de shadcn esté instalado. Si no existe, instalarlo:
> ```bash
> cd Frontend && npx shadcn@latest add label
> ```

**Step 2: Compilar**

```bash
cd Frontend && npm run build 2>&1 | head -40
```

**Step 3: Commit**

```bash
git add Frontend/src/features/orders/components/ordersHistory/OrdersFiltersBar.tsx
git commit -m "feat: OrdersFiltersBar con filtros de fecha, estado y pago"
```

---

## Task 11: Crear `OrdersTableRow.tsx` y `OrdersTable.tsx`

**Files:**
- Create: `Frontend/src/features/orders/components/ordersHistory/OrdersTableRow.tsx`
- Create: `Frontend/src/features/orders/components/ordersHistory/OrdersTable.tsx`

**Step 1: Crear `OrdersTableRow.tsx`**

```tsx
import { Eye } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Button } from '@/shared/components/ui/button';
import { cn } from '@/shared/utils/cn';
import { TABLE_HEADER_CLASS as TH } from '@/shared/utils/constants';
import type { Order } from '@/features/orders/types/order';

interface OrdersTableRowProps {
  order: Order;
  onView: (orderId: number) => void;
}

const PAYMENT_LABELS: Record<string, { label: string; className: string }> = {
  paid:    { label: 'Pagado',    className: 'text-green-700 bg-green-50 border-green-200' },
  partial: { label: 'Parcial',   className: 'text-amber-700 bg-amber-50 border-amber-200' },
  pending: { label: 'Pendiente', className: 'text-red-700 bg-red-50 border-red-200'   },
};

export function OrdersTableRow({ order, onView }: OrdersTableRowProps) {
  const payment = PAYMENT_LABELS[order.paymentStatus] ?? PAYMENT_LABELS.pending;

  return (
    <>
      {/* Desktop row */}
      <div className="hidden md:grid grid-cols-[80px_2fr_1fr_80px_1fr_120px_48px] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors">
        <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
          #{order.id}
        </span>
        <p className="text-sm font-medium text-zinc-800 truncate">
          {order.client?.name ?? '—'}
        </p>
        <p className="text-xs text-zinc-500 capitalize">
          {format(new Date(order.promisedDate), "d 'de' MMM", { locale: es })}
        </p>
        <span className="text-xs text-zinc-500 font-mono">
          {order.items.length}
        </span>
        <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
          ${order.total.toFixed(2)}
        </span>
        <span className={cn('text-[11px] font-medium px-2 py-0.5 rounded-full border w-fit', payment.className)}>
          {payment.label}
        </span>
        <div className="flex justify-end">
          <Button
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            onClick={() => onView(order.id)}
          >
            <Eye className="h-3.5 w-3.5 text-zinc-400" />
          </Button>
        </div>
      </div>

      {/* Mobile card */}
      <div className="md:hidden px-6 py-4 border-b border-zinc-100">
        <div className="flex items-start justify-between gap-3 mb-1.5">
          <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">
            #{order.id}
          </span>
          <div className="flex items-center gap-2">
            <span className={cn('text-[11px] font-medium px-2 py-0.5 rounded-full border', payment.className)}>
              {payment.label}
            </span>
            <span className="font-mono font-semibold tabular-nums text-sm text-zinc-900">
              ${order.total.toFixed(2)}
            </span>
          </div>
        </div>
        <p className="text-sm font-medium text-zinc-800">{order.client?.name ?? '—'}</p>
        <p className="text-xs text-zinc-400 capitalize mt-0.5">
          Entrega: {format(new Date(order.promisedDate), "d 'de' MMM 'de' yyyy", { locale: es })}
        </p>
        <div className="flex gap-2 mt-3 pt-3 border-t border-zinc-100">
          <Button
            size="sm"
            variant="outline"
            className="h-7 text-xs"
            onClick={() => onView(order.id)}
          >
            <Eye className="h-3 w-3 mr-1" />
            Ver detalle
          </Button>
        </div>
      </div>
    </>
  );
}
```

**Step 2: Crear `OrdersTable.tsx`**

```tsx
import { ChevronLeft, ChevronRight, Package } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { TABLE_HEADER_CLASS as TH } from '@/shared/utils/constants';
import { OrdersTableRow } from './OrdersTableRow';
import type { Order } from '@/features/orders/types/order';

interface OrdersTableProps {
  orders: Order[];
  isLoading: boolean;
  error: string | null;
  totalCount: number;
  page: number;
  pageSize: number;
  hasActiveFilters: boolean;
  onPageChange: (page: number) => void;
  onViewOrder: (orderId: number) => void;
  onClearFilters: () => void;
  onRetry: () => void;
}

function SkeletonRow() {
  return (
    <div className="hidden md:grid grid-cols-[80px_2fr_1fr_80px_1fr_120px_48px] gap-4 items-center px-6 py-3 border-b border-zinc-100">
      {[80, 160, 80, 40, 80, 70, 28].map((w, i) => (
        <div key={i} className="h-4 bg-zinc-100 rounded animate-pulse" style={{ width: w }} />
      ))}
    </div>
  );
}

export function OrdersTable({
  orders,
  isLoading,
  error,
  totalCount,
  page,
  pageSize,
  hasActiveFilters,
  onPageChange,
  onViewOrder,
  onClearFilters,
  onRetry,
}: OrdersTableProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  if (error) {
    return (
      <div className="py-16 flex flex-col items-center gap-3">
        <p className="text-sm text-zinc-500">{error}</p>
        <Button size="sm" variant="outline" onClick={onRetry}>
          Reintentar
        </Button>
      </div>
    );
  }

  if (isLoading) {
    return (
      <>
        <div className="hidden md:grid grid-cols-[80px_2fr_1fr_80px_1fr_120px_48px] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
          {['Folio', 'Cliente', 'Entrega', 'Items', 'Total', 'Pago', ''].map((h) => (
            <p key={h} className={TH}>{h}</p>
          ))}
        </div>
        {Array.from({ length: 5 }).map((_, i) => <SkeletonRow key={i} />)}
      </>
    );
  }

  if (orders.length === 0) {
    return (
      <div className="py-16 flex flex-col items-center gap-3">
        <div className="w-10 h-10 rounded-full bg-zinc-50 border border-zinc-100 flex items-center justify-center">
          <Package className="h-4 w-4 text-zinc-300" />
        </div>
        <div className="text-center">
          <p className="text-sm font-medium text-zinc-400">
            {hasActiveFilters ? 'No hay órdenes con estos filtros' : 'Sin órdenes'}
          </p>
        </div>
        {hasActiveFilters && (
          <Button size="sm" variant="outline" onClick={onClearFilters}>
            Limpiar filtros
          </Button>
        )}
      </div>
    );
  }

  return (
    <>
      {/* Table headers */}
      <div className="hidden md:grid grid-cols-[80px_2fr_1fr_80px_1fr_120px_48px] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
        {['Folio', 'Cliente', 'Entrega', 'Items', 'Total', 'Pago', ''].map((h) => (
          <p key={h} className={TH}>{h}</p>
        ))}
      </div>

      {/* Rows */}
      {orders.map((order) => (
        <OrdersTableRow key={order.id} order={order} onView={onViewOrder} />
      ))}

      {/* Pagination + footer */}
      <div className="px-6 py-3 bg-zinc-50 border-t border-zinc-100 flex items-center justify-between">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
          {totalCount} orden{totalCount !== 1 ? 'es' : ''}
        </p>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0"
            disabled={page <= 1}
            onClick={() => onPageChange(page - 1)}
          >
            <ChevronLeft className="h-3.5 w-3.5" />
          </Button>
          <span className="text-xs text-zinc-500 tabular-nums">
            Página {page} de {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            className="h-7 w-7 p-0"
            disabled={page >= totalPages}
            onClick={() => onPageChange(page + 1)}
          >
            <ChevronRight className="h-3.5 w-3.5" />
          </Button>
        </div>
      </div>
    </>
  );
}
```

**Step 3: Compilar**

```bash
cd Frontend && npm run build 2>&1 | head -40
```

**Step 4: Commit**

```bash
git add Frontend/src/features/orders/components/ordersHistory/OrdersTableRow.tsx
git add Frontend/src/features/orders/components/ordersHistory/OrdersTable.tsx
git commit -m "feat: OrdersTableRow y OrdersTable con paginación y skeleton"
```

---

## Task 12: Crear `OrdersExportButton.tsx` y reescribir `OrdersList.tsx`

**Files:**
- Create: `Frontend/src/features/orders/components/ordersHistory/OrdersExportButton.tsx`
- Modify: `Frontend/src/features/orders/pages/OrdersList.tsx`

**Step 1: Crear `OrdersExportButton.tsx`**

```tsx
import { useState } from 'react';
import { Download, FileSpreadsheet, FileText, Loader2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { useToast } from '@/shared/hooks/use-toast';
import { exportOrders } from '@/api/orders';
import type { OrderHistoryFilters } from '@/features/orders/types/order';

interface OrdersExportButtonProps {
  activeFilters: OrderHistoryFilters;
  disabled?: boolean;
}

export function OrdersExportButton({ activeFilters, disabled }: OrdersExportButtonProps) {
  const [isExporting, setIsExporting] = useState(false);
  const { toast } = useToast();

  async function handleExport(format: 'xlsx' | 'pdf') {
    setIsExporting(true);
    try {
      await exportOrders(format, activeFilters);
    } catch {
      toast({
        variant: 'destructive',
        title: 'No se pudo generar el reporte',
        description: 'Intenta de nuevo o contacta soporte si el problema persiste.',
      });
    } finally {
      setIsExporting(false);
    }
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="h-8 text-xs"
          disabled={disabled || isExporting}
        >
          {isExporting ? (
            <Loader2 className="h-3.5 w-3.5 mr-1.5 animate-spin" />
          ) : (
            <Download className="h-3.5 w-3.5 mr-1.5" />
          )}
          Exportar
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          disabled={isExporting}
          onClick={() => handleExport('xlsx')}
          className="text-xs cursor-pointer"
        >
          <FileSpreadsheet className="h-3.5 w-3.5 mr-2 text-green-600" />
          Exportar Excel
        </DropdownMenuItem>
        <DropdownMenuItem
          disabled={isExporting}
          onClick={() => handleExport('pdf')}
          className="text-xs cursor-pointer"
        >
          <FileText className="h-3.5 w-3.5 mr-2 text-red-500" />
          Exportar PDF
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

> Nota: Este componente requiere el componente `DropdownMenu` de shadcn. Si no está instalado:
> ```bash
> cd Frontend && npx shadcn@latest add dropdown-menu
> ```

**Step 2: Reescribir `OrdersList.tsx`**

```tsx
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, DollarSign } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { useOrdersStore } from '@/features/orders/stores/ordersStore';
import { OrderSearchSheet } from '@/features/orders/components/orderSearch/OrderSearchSheet';
import { OrdersFiltersBar } from '@/features/orders/components/ordersHistory/OrdersFiltersBar';
import { OrdersTable } from '@/features/orders/components/ordersHistory/OrdersTable';
import { OrdersExportButton } from '@/features/orders/components/ordersHistory/OrdersExportButton';
import { useUIStore } from '@/shared/stores/uiStore';
import type { OrderHistoryFilters } from '@/features/orders/types/order';

export function OrdersList() {
  const navigate = useNavigate();
  const openCashClosing = useUIStore((state) => state.openCashClosing);
  const { orders, isLoading, error, pagination, activeFilters, fetchOrders, clearFilters } =
    useOrdersStore();

  const [searchSheetOpen, setSearchSheetOpen] = useState(false);
  const [selectedOrderId, setSelectedOrderId] = useState<number | undefined>(undefined);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  function handleFiltersChange(filters: OrderHistoryFilters) {
    fetchOrders(filters, 1);
  }

  function handlePageChange(page: number) {
    fetchOrders(activeFilters, page);
  }

  function handleViewOrder(orderId: number) {
    setSelectedOrderId(orderId);
    setSearchSheetOpen(true);
  }

  function handleRetry() {
    fetchOrders(activeFilters, pagination.page);
  }

  const hasActiveFilters =
    !!activeFilters.startDate ||
    !!activeFilters.endDate ||
    (activeFilters.statusIds?.length ?? 0) > 0 ||
    (activeFilters.paymentStatuses?.length ?? 0) > 0;

  return (
    <div className="bg-white border border-zinc-200 rounded-lg overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
        <div>
          <h1 className="text-xl font-semibold text-zinc-900 tracking-tight">Órdenes</h1>
          <p className="text-xs text-zinc-400 mt-0.5">Gestión de órdenes de servicio</p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => openCashClosing()}
            className="h-8 text-xs"
          >
            <DollarSign className="h-3.5 w-3.5 mr-1.5" />
            Corte de Caja
          </Button>
          <OrdersExportButton activeFilters={activeFilters} />
          <Button
            size="sm"
            onClick={() => navigate('/orders/new')}
            className="h-8 text-xs"
          >
            <Plus className="h-3.5 w-3.5 mr-1.5" />
            Nueva Venta
          </Button>
        </div>
      </div>

      {/* Filters */}
      <OrdersFiltersBar onFiltersChange={handleFiltersChange} />

      {/* Table */}
      <OrdersTable
        orders={orders}
        isLoading={isLoading}
        error={error}
        totalCount={pagination.totalCount}
        page={pagination.page}
        pageSize={pagination.pageSize}
        hasActiveFilters={hasActiveFilters}
        onPageChange={handlePageChange}
        onViewOrder={handleViewOrder}
        onClearFilters={clearFilters}
        onRetry={handleRetry}
      />

      <OrderSearchSheet
        open={searchSheetOpen}
        onOpenChange={setSearchSheetOpen}
        initialOrderId={selectedOrderId}
      />
    </div>
  );
}
```

**Step 3: Compilar**

```bash
cd Frontend && npm run build 2>&1 | head -60
```
Expected: Build succeeded, 0 errors de tipo.

**Step 4: Probar en desarrollo**

```bash
cd Frontend && npm run dev
```
- Abrir `http://localhost:5173/orders`
- Verificar que carga la primera página de 20 órdenes
- Probar filtro de fechas → la tabla debe actualizarse
- Probar paginación → la tabla debe mostrar la siguiente página
- Probar exportar Excel → debe descargarse un `.xlsx`

**Step 5: Commit final**

```bash
git add Frontend/src/features/orders/components/ordersHistory/OrdersExportButton.tsx
git add Frontend/src/features/orders/pages/OrdersList.tsx
git commit -m "feat: historial de órdenes con filtros, paginación y exportación"
```

---

## Checklist de verificación final

- [ ] `GET /api/orders?page=1&pageSize=20` devuelve `{ data, totalCount, page, pageSize }`
- [ ] `GET /api/orders?statusIds=1&statusIds=2` filtra por estado de orden
- [ ] `GET /api/orders?paymentStatuses=paid&paymentStatuses=pending` filtra por estado de pago
- [ ] `GET /api/orders?startDate=2026-01-01&endDate=2026-03-31` filtra por rango de fechas
- [ ] `GET /api/reportes/ordenes/export?format=xlsx` descarga un `.xlsx` válido
- [ ] `GET /api/reportes/ordenes/export?format=pdf` descarga un `.pdf` válido
- [ ] Cambiar filtro en la UI resetea a página 1
- [ ] El botón Exportar exporta con los filtros activos
- [ ] El skeleton de carga aparece durante la petición
- [ ] Empty state muestra "Limpiar filtros" cuando hay filtros activos
