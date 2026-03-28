# Diseño: Rediseño Página de Servicios

**Fecha:** 2026-02-27
**Estado:** Aprobado

---

## Contexto

El módulo de servicios fue reconstruido a nivel de datos para alinearse con el schema real de SQL Server. Ahora tiene 5 entidades con relaciones claras. El layout de pestañas (Tabs) se reemplaza por un **rail lateral + panel**, más profesional y escalable.

**Entidades actuales:**

```
Category       — name, description, isActive
Service        — categoryId, code, name, chargeType (ByWeight|ByPiece),
                 pricePerKg?, minWeight?, maxWeight?, estimatedTime?, isActive
GarmentType    — name, description, isActive  (catálogo global)
ServiceGarment — serviceId, garmentTypeId, unitPrice, isActive  (junction ByPiece)
Discount       — name, type (Percentage|FixedAmount), value, startDate, endDate?, isActive
```

---

## Layout General

### Estructura

```
ServicesList.tsx
├── ServicesRail          (columna izquierda, fija ~220px)
└── ServicesPanel         (columna derecha, flex-1)
    ├── ServicesSection
    ├── CategoriesSection
    ├── GarmentTypesSection
    ├── PricesSection
    └── DiscountsSection
```

### Grid

```tsx
<div className="flex h-full">
  <ServicesRail />              {/* w-[220px] border-r border-zinc-200 shrink-0 */}
  <div className="flex-1 overflow-y-auto">
    {/* panel activo según sección seleccionada */}
  </div>
</div>
```

El rail y el panel comparten `bg-white`. La separación es únicamente `border-r border-zinc-200`.

---

## Rail Lateral

### Anatomía

```
┌────────────────────┐
│                    │
│  CATÁLOGO          │  ← label grupo: text-[10px] tracking-widest uppercase text-zinc-400
│  Servicios         │  ← item activo: border-l-2 border-zinc-900 text-zinc-900 font-medium
│  Categorías        │  ← item inactivo: text-zinc-500 hover:text-zinc-700 hover:bg-zinc-50
│                    │
│  PRENDAS           │
│  Tipos de Prenda   │
│  Precios           │
│                    │
│  COMERCIAL         │
│  Descuentos        │
│                    │
└────────────────────┘
```

### Grupos y secciones

| Grupo | Sección | Entidad gestionada |
|---|---|---|
| CATÁLOGO | Servicios | `Service` |
| CATÁLOGO | Categorías | `Category` |
| PRENDAS | Tipos de Prenda | `GarmentType` |
| PRENDAS | Precios | `ServiceGarment` (junction) |
| COMERCIAL | Descuentos | `Discount` |

### Comportamiento responsive

- **Desktop (≥ lg):** Rail fijo a la izquierda
- **Mobile (< lg):** Rail se convierte en `<Select>` o pills horizontales encima del panel

### Tokens

```
Rail container:     w-[220px] border-r border-zinc-200 py-6 shrink-0
Grupo label:        px-4 mb-1 mt-5 first:mt-0 text-[10px] font-semibold tracking-widest uppercase text-zinc-400
Item inactivo:      px-4 py-2 text-sm text-zinc-500 hover:text-zinc-800 hover:bg-zinc-50 cursor-pointer rounded-none transition-colors
Item activo:        px-4 py-2 text-sm text-zinc-900 font-medium bg-zinc-50 border-l-2 border-zinc-900
```

---

## Panel de Contenido

Cada sección sigue el mismo patrón:

```
┌─────────────────────────────────────────────────────────┐
│  [Header: título + contador + botón "+ Nuevo"]          │  pb-4 border-b border-zinc-100
│─────────────────────────────────────────────────────────│
│  [Tabla: columnas relevantes + acciones por fila]       │  ledger rows
└─────────────────────────────────────────────────────────┘
```

### Header del panel

```tsx
<div className="flex items-center justify-between px-6 py-5 border-b border-zinc-100">
  <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
    Nombre Sección ({count})
  </p>
  <Button size="sm" className="bg-zinc-900 hover:bg-zinc-800 text-white">
    <Plus className="h-4 w-4 mr-1" />
    Nuevo ...
  </Button>
</div>
```

### Tabla (ledger pattern)

Columnas, filas con `border-b border-zinc-100 hover:bg-zinc-50`. Acciones al final: lápiz + papelera.

---

## Diseño por Sección

### 1. Servicios

**Columnas:** Código · Nombre · Categoría · Tipo · Precio/kg · Estado · Acciones

```
Código   Nombre             Categoría    Tipo       Precio/kg  Estado    Acciones
LAV-01   Lavado estándar    Lavandería   Por kilo   $18.00     ● Activo  ✏ 🗑
LAV-02   Lavado express     Lavandería   Por kilo   $25.00     ● Activo  ✏ 🗑
TIN-01   Tintorería         Tintorería   Por pieza     —       ● Activo  ✏ 🗑
PLA-01   Planchado          Planchado    Por kilo   $12.00     ○ Inactivo ✏ 🗑
```

- Servicios `ByPiece` muestran `—` en Precio/kg (sin precio fijo, el precio es por prenda)
- El form modal incluye: código, nombre, descripción, categoría (Select), tipo de cobro (Select), y condicional: si ByWeight → precio/kg + min/max peso; si ByPiece → no precio. Tiempo estimado siempre visible.

### 2. Categorías

**Columnas:** Nombre · Descripción · Estado · Acciones

```
Nombre         Descripción              Estado      Acciones
Lavandería     Servicios de lavado      ● Activo    ✏ 🗑
Tintorería     Limpieza en seco         ● Activo    ✏ 🗑
Planchado      —                        ● Activo    ✏ 🗑
```

- Form modal: nombre, descripción (opcional), checkbox isActive (solo en edición)

### 3. Tipos de Prenda

**Columnas:** Nombre · Descripción · Estado · Acciones

```
Nombre      Descripción        Estado      Acciones
Camisa      —                  ● Activo    ✏ 🗑
Pantalón    Pantalones y jeans ● Activo    ✏ 🗑
Edredón     —                  ● Activo    ✏ 🗑
```

- Form modal: nombre, descripción (opcional), checkbox isActive (solo en edición)
- Son **globales** — no están ligados a ningún servicio específico aquí

### 4. Precios (ServiceGarment)

**Columnas:** Servicio · Tipo de Prenda · Precio Unitario · Estado · Acciones

```
Servicio         Tipo de Prenda   Precio Unitario   Estado      Acciones
Tintorería       Camisa           $8.00             ● Activo    ✏ 🗑
Tintorería       Pantalón         $7.50             ● Activo    ✏ 🗑
Tintorería       Edredón          $22.00            ● Activo    ✏ 🗑
```

- Tabla plana (no agrupada) con columna "Servicio" visible — más fácil de escanear
- **Filtro por servicio** encima de la tabla (Select) — opcional, útil cuando hay muchos precios
- Form modal (alta): Select servicio (ByPiece only) + Select tipo de prenda + input precio
- Form modal (edición): solo precio unitario + checkbox isActive
- Botón "+ Nuevo Precio" en el header del panel

### 5. Descuentos

**Columnas:** Nombre · Tipo · Valor · Vigencia · Estado · Acciones

```
Nombre           Tipo        Valor   Vigencia         Estado      Acciones
Cliente frecuente Porcentaje  10%    01/26 — ∞        ● Activo    ✏ 🗑
Promo lunes      Monto Fijo  $5.00  01/26 — 03/26     ● Activo    ✏ 🗑
```

- Form modal: nombre, tipo (Select), valor (condicional: % o $), startDate (required), endDate (opcional), checkbox isActive (edición)

---

## CRUD Pattern (común a todas las secciones)

| Acción | Trigger | Componente |
|---|---|---|
| Alta | Botón "+ Nuevo" en header | `<Dialog>` con form (patrón system.md) |
| Edición | Ícono lápiz en fila | Mismo `<Dialog>` precargado |
| Eliminación | Ícono papelera en fila | `<AlertDialog>` de confirmación |
| Visualización | Tabla siempre visible | Ledger rows |

**Forms**: Componente `*FormContent` separado dentro del Dialog (se desmonta al cerrar → reset automático del form).

---

## Estado de Selección del Rail

Estado local en `ServicesList.tsx`:

```typescript
type ServiceSection = 'services' | 'categories' | 'garmentTypes' | 'prices' | 'discounts';
const [activeSection, setActiveSection] = useState<ServiceSection>('services');
```

---

## Responsive

```
lg+:  flex-row (rail izq + panel der)
< lg: flex-col (selector arriba + panel abajo)
      Rail → <Select> con las 5 opciones
```

---

## Archivos a crear/modificar

| Archivo | Acción |
|---|---|
| `pages/ServicesList.tsx` | Reescribir completo (layout rail + panel) |
| `components/ServicesRail.tsx` | Nuevo — navegación lateral |
| `components/ServicesSection.tsx` | Extraer de ServicesTab.tsx existente |
| `components/CategoriesSection.tsx` | Nuevo (antes no tenía UI propia) |
| `components/GarmentTypesSection.tsx` | Renombrar/refactorizar GarmentsTab.tsx |
| `components/PricesSection.tsx` | Refactorizar PricesTab.tsx (tabla plana + filtro) |
| `components/DiscountsSection.tsx` | Renombrar/refactorizar DiscountsTab.tsx |

Los 4 archivos `*Tab.tsx` existentes se convierten en `*Section.tsx` — mismo contenido, diferente nombre para reflejar que ya no son pestañas.
