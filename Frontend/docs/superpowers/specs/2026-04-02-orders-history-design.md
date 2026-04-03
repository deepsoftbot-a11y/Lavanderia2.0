# Historial de Órdenes — Diseño

**Fecha:** 2026-04-02
**Alcance:** Reemplazar `OrdersList.tsx` con un módulo de historial paginado, filtrable y con exportación a Excel/PDF.

---

## Contexto

La página actual `OrdersList.tsx` carga todas las órdenes sin filtros ni paginación. Conforme crezca el volumen de datos esto se volverá inviable. Este módulo la reemplaza con paginación en backend, tres filtros específicos y descarga de reporte con los datos filtrados.

---

## Decisiones de diseño

- **Paginación en backend** (no en frontend): el backend aplica `LIMIT/OFFSET` en Dapper. El frontend solo recibe la página solicitada.
- **Filtros**: rango de fechas, estado de orden y estado de pago. Sin búsqueda por texto ni por cliente (fuera de alcance de este módulo).
- **Reporte**: exporta **todos** los registros que coincidan con los filtros activos, ignorando la paginación. Formatos: Excel (.xlsx) y PDF.
- **Tamaño de página**: 20 registros por página (fijo, sin selector).

---

## Backend

### 1. Tipo compartido `PagedResult<T>`

Ubicación: `LaundryManagement.Application/Common/PagedResult.cs`

```csharp
public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize
);
```

### 2. Cambios en `GetOrdersQuery`

```csharp
public record GetOrdersQuery(
    string? Search,
    int? ClientId,
    DateTime? StartDate,
    DateTime? EndDate,
    int[]? StatusIds,          // nuevo: filtro por estado de orden
    string[]? PaymentStatuses, // nuevo: ["paid", "partial", "pending"]
    string SortBy = "createdAt",
    string SortOrder = "desc",
    int Page = 1,              // nuevo
    int PageSize = 20          // nuevo
) : IRequest<PagedResult<OrderResponseDto>>;
```

### 3. Cambios en `IOrderRepository`

El método `GetAllAsync` retorna una tupla con los items y el conteo total:

```csharp
Task<(IEnumerable<OrderPure> Items, int TotalCount)> GetAllAsync(
    string? search,
    int? clientId,
    DateTime? startDate,
    DateTime? endDate,
    int[]? statusIds,
    string[]? paymentStatuses,
    string sortBy,
    string sortOrder,
    int page,
    int pageSize,
    CancellationToken cancellationToken
);
```

La implementación Dapper ejecuta dos queries:
- `SELECT COUNT(*) FROM ...` con los filtros activos.
- `SELECT ... FROM ... LIMIT @PageSize OFFSET @Offset` con los mismos filtros.

### 4. Handler actualizado

`GetOrdersQueryHandler` construye el `PagedResult<OrderResponseDto>` combinando los datos mapeados con `totalCount`, `page` y `pageSize`.

### 5. Endpoint de exportación

Nuevo endpoint en `ReportesController`:

```
GET /api/reportes/ordenes/export
Query params: format (xlsx|pdf), startDate, endDate, statusIds, paymentStatuses
Response: archivo binario con Content-Disposition: attachment
```

- `format=xlsx` → ClosedXML
- `format=pdf` → QuestPDF

La query de exportación reutiliza los mismos filtros de `GetOrdersQuery` pero **sin** `Page`/`PageSize` (trae todos los registros que coincidan).

El reporte incluye columnas: Folio, Cliente, Fecha Creación, Fecha Entrega Prometida, Estado Orden, Estado Pago, Subtotal, Descuento, Total, Pagado, Saldo.

---

## Frontend

### Estructura de archivos

```
src/features/orders/
├── pages/
│   └── OrdersList.tsx                   ← reemplazar completamente
├── components/
│   └── ordersHistory/                   [nuevo]
│       ├── OrdersFiltersBar.tsx         [nuevo]
│       ├── OrdersTable.tsx              [nuevo]
│       ├── OrdersTableRow.tsx           [nuevo]
│       └── OrdersExportButton.tsx       [nuevo]
├── stores/
│   └── ordersStore.ts                   ← extender con paginación y filtros activos
└── types/
    └── order.ts                         ← agregar PagedResult y OrderHistoryFilters
```

### Tipos nuevos en `order.ts`

```typescript
export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface OrderHistoryFilters {
  startDate?: string;       // ISO date string: "2026-01-01"
  endDate?: string;
  statusIds?: number[];
  paymentStatuses?: PaymentStatus[];
}
```

### Estado nuevo en `ordersStore`

```typescript
pagination: {
  page: number;       // página actual
  pageSize: number;   // siempre 20
  totalCount: number; // total de registros con filtros activos
};
activeFilters: OrderHistoryFilters;
```

`fetchOrders(filters, page)` actualiza `orders`, `pagination` y `activeFilters` en una sola operación.

### `OrdersFiltersBar`

Barra compacta con tres controles:

1. **Rango de fechas**: dos `<Input type="date">` (Desde / Hasta). Si `endDate < startDate`, muestra error inline y deshabilita la búsqueda.
2. **Estado de orden**: `<Select>` con opción "Todos" + estados individuales. Los valores se definen como constantes en el frontend (Pendiente=1, En proceso=2, Listo=3, Entregado=4, Cancelado=5) — no requieren llamada a API.
3. **Estado de pago**: tres `<Badge>` toggleables — Pagado / Parcial / Pendiente.
4. Botón **Limpiar filtros** visible solo cuando hay algún filtro activo.

Al cambiar cualquier control se dispara `fetchOrders({ ...filters, page: 1 })` (reset a primera página).

### `OrdersTable`

Mantiene el diseño visual actual (tabla en desktop, cards en mobile). Debajo de la tabla:

```
← Anterior    Página 3 de 12    Siguiente →
```

- "Anterior" deshabilitado en página 1.
- "Siguiente" deshabilitado en la última página.
- Mientras carga: skeleton de 5 filas (no spinner de pantalla completa).

### `OrdersExportButton`

Dropdown con dos opciones: **Exportar Excel** y **Exportar PDF**.

- Lee `activeFilters` del store (sin `page`/`pageSize`).
- Llama a `GET /api/reportes/ordenes/export?format=xlsx&...` usando `fetch` + `blob`.
- Durante la descarga: spinner en el botón, opciones deshabilitadas.
- En error: toast "No se pudo generar el reporte".

### `OrdersList.tsx` (reemplazado)

Orquesta los cuatro componentes:

```tsx
export function OrdersList() {
  return (
    <div>
      <Header />              {/* título + botones Nueva Venta, Corte de Caja */}
      <OrdersFiltersBar />
      <OrdersTable />
      <OrdersExportButton />  {/* posición: junto al header, a la derecha de los botones existentes */}
    </div>
  );
}
```

---

## Flujo de datos

### Carga normal

```
Usuario cambia filtro / cambia página
  → fetchOrders({ ...filters, page })
  → GET /api/ordenes/v2?startDate=&statusIds=&page=1&pageSize=20
  → Handler: COUNT(*) + SELECT ... LIMIT 20 OFFSET 0
  → { data: [...20 items], totalCount: 340, page: 1, pageSize: 20 }
  → Store actualiza orders + pagination + activeFilters
  → OrdersTable re-renderiza
```

### Exportación

```
Clic en "Exportar Excel"
  → Lee activeFilters del store
  → GET /api/reportes/ordenes/export?format=xlsx&startDate=&...
  → Backend: SELECT sin LIMIT, genera .xlsx con ClosedXML
  → Content-Disposition: attachment; filename="ordenes-2026-04-02.xlsx"
  → fetch → blob → URL.createObjectURL → <a>.click()
```

---

## Manejo de errores

| Escenario | Comportamiento |
|---|---|
| Error de red al cargar | Toast de error + tabla muestra empty state con botón "Reintentar" |
| Sin resultados con filtros activos | Empty state: "No hay órdenes con estos filtros" + botón "Limpiar filtros" |
| Error al exportar | Toast "No se pudo generar el reporte" |
| `endDate` < `startDate` | Error inline en la barra, botón Exportar deshabilitado |
| Página solicitada fuera de rango | Reset automático a página 1 |

---

## Fuera de alcance

- Búsqueda por folio o cliente (ya existe en `OrderSearchSheet`)
- Selector de tamaño de página
- Ordenamiento por columna interactivo (el backend ya soporta `sortBy`/`sortOrder` pero la UI no lo expone en este módulo)
- Vista de detalle de orden desde el historial (se reutiliza el `OrderSearchSheet` existente)
