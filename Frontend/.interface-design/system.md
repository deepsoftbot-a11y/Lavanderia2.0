# Interface Design System — AppWebLavanderia

## Product Context

**Dominio:** Lavandería / punto de venta (POS). Operadores internos: cajeros, administradores.
**Usuario típico:** Cajero al final del turno — cansado, contando efectivo, necesita precisión y rapidez.
**Tono:** Profesional, financiero. Como un terminal POS o software contable. No SaaS genérico.

---

## Direction & Feel

**No:** SaaS azul genérico, cards con shadow dramáticas, accent colorido.
**Sí:** Zinc/slate como base estructural. Color solo para semántica (resultado, estado, error). Sensación de terminal financiero.

---

## Depth Strategy

**Borders-only.** Sin box-shadow en componentes de layout.

- Separación entre secciones: `border-b border-zinc-100`
- Énfasis: `border-zinc-200`
- Sin `shadow-md`, `shadow-lg` en cards o modales

---

## Surfaces (escala de elevación)

| Nivel | Uso | Token Tailwind |
|-------|-----|---------------|
| Base  | Fondo principal de modales/cards | `bg-white` |
| Recessed | Secciones de solo lectura, footers, headers secundarios | `bg-zinc-50` |
| Inset | Referencias, etiquetas destacadas dentro de secciones | `bg-zinc-100` |
| Semántico | Diferencia positiva | `bg-emerald-50` |
| Semántico | Diferencia negativa | `bg-rose-50` |

Regla: **mismo tono (zinc), solo varía la lightness**. No mezclar hues en superficies de layout.

---

## Color Palette

```
Texto primario:    text-zinc-900
Texto secundario:  text-zinc-500
Texto muted:       text-zinc-400
Labels sección:    text-[10px] font-semibold tracking-widest uppercase text-zinc-400

Éxito / positivo:  text-emerald-600 / bg-emerald-50 / border-emerald-100
Error / negativo:  text-rose-600    / bg-rose-50    / border-rose-100
Neutral semántico: bg-zinc-100 text-zinc-400

Botón primario:    bg-primary (azul Chatgot hsl(228 100% 60%) ≈ #4664FF) — usar <Button> sin className

Paleta de inputs (estilo filled):
  Normal:          bg-zinc-100 border-2 border-transparent
  Focus:           focus:bg-blue-50 focus:border-blue-600
  Texto input:     text-indigo-900  → hsl(242 65% 15%)
  Placeholder:     placeholder:text-zinc-400
  Error:           bg-rose-50 border-rose-400 text-rose-900
  Deshabilitado:   opacity-50 cursor-not-allowed
```

Color con significado, nunca decorativo.

---

## Typography

- **Montos monetarios:** `font-mono tabular-nums` — alineación decimal precisa
- **Labels de sección:** `text-[10px] font-semibold tracking-widest uppercase`
- **Labels de campo:** `text-xs text-zinc-500 font-medium`
- **Títulos de modal:** `text-sm font-semibold text-zinc-900 tracking-tight`
- **Subtítulo / fecha:** `text-xs text-zinc-400 capitalize`
- **Hero number (resultado clave):** `font-mono font-bold tabular-nums text-3xl tracking-tight leading-none`

---

## Spacing

Base unit: 4px (`1` en Tailwind).

| Contexto | Valor |
|----------|-------|
| Padding interno de sección | `px-6 py-4` |
| Padding header/footer de modal | `px-6 pt-5 pb-4` / `px-6 py-4` |
| Gap entre inputs en grid | `gap-3` |
| Gap entre items de método de pago | `gap-2` |
| Espaciado entre label y campo | `space-y-1` |

---

## Modal Pattern

```tsx
<DialogContent className="max-w-md p-0 gap-0 overflow-hidden">
  {/* Header fijo */}
  <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
    <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight" />
    {/* subtítulo opcional */}
  </DialogHeader>

  {/* Contenido scrollable */}
  <div className="flex flex-col max-h-[82vh]">
    <div className="overflow-y-auto flex-1">
      {/* Sección de solo lectura */}
      <div className="px-6 py-4 bg-zinc-50 border-b border-zinc-100">
        <p className="text-[10px] font-semibold tracking-widest text-zinc-400 uppercase mb-3">
          Título Sección
        </p>
        {/* contenido */}
      </div>

      {/* Sección de inputs */}
      <div className="px-6 py-4 border-b border-zinc-100">
        {/* contenido */}
      </div>
    </div>

    {/* Footer fijo */}
    <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
      <Button variant="outline">Cancelar</Button>
      <Button>Confirmar</Button>
    </div>
  </div>
</DialogContent>
```

---

## Input System — Tokens Base

Todos los inputs del proyecto siguen el mismo token set. **Filled style**: fondo gris en reposo, sin borde visible; al activarse aparece borde azul + fondo azul muy claro.

| Estado | Clases |
|--------|--------|
| Normal | `bg-zinc-100 border-2 border-transparent rounded-xl h-11 px-4 text-indigo-900 placeholder:text-zinc-400` |
| Hover  | `hover:bg-zinc-200` |
| Focus  | `focus:border-blue-600 focus:bg-blue-50` |
| Error  | `border-rose-400 bg-rose-50 text-rose-900` |
| Disabled | `opacity-50 cursor-not-allowed` |

**Componentes del sistema:**
- `<Input>` — input base de Shadcn, uso genérico
- `<FieldInput>` — igual con prop `hasError`
- `<ClearableInput>` — FieldInput + botón X para limpiar
- `<PasswordInput>` — FieldInput + toggle de visibilidad
- `<CurrencyInput>` — filled style + prefijo `$` + font-mono + `text-right`
- `<SelectTrigger>` — mismo token set; abierto muestra estado focus
- `<Textarea>` — mismo token set, sin `h-11`, con `py-3 min-h-[88px]`
- `CommandInput` — bg-transparent dentro de Command, `placeholder:text-zinc-400`
- `CustomerSelector` trigger — mismo token set que SelectTrigger

**Dropdown containers** (SelectContent, PopoverContent con Command):
```
rounded-xl border border-zinc-200 bg-white shadow-sm
```
Sin borde azul, sin shadow dramática. Solo zinc estructural.

---

## Input Monetario (CurrencyInput)

Usar el componente `<CurrencyInput>` — ya implementa filled style + prefijo `$` + font-mono + `text-right`.
Acepta `value: number | ''` y `onChange: (value: number | '') => void`. Prop `hasError` disponible.

```tsx
import { CurrencyInput } from '@/shared/components/ui/currency-input';

<CurrencyInput value={amount} onChange={setAmount} />
<CurrencyInput value={amount} onChange={setAmount} max={total} hasError={!!errors.amount} />
```

---

## Resultado / Hero Semántico

Para cualquier resultado clave que cambie en tiempo real (diferencia, total, balance):

```tsx
<div className={cn(
  'px-6 py-5 border-b transition-colors duration-150',
  value === 0 && 'bg-white border-zinc-100',
  value > 0  && 'bg-emerald-50 border-emerald-100',
  value < 0  && 'bg-rose-50 border-rose-100',
)}>
  <div className="flex items-end justify-between">
    <div>
      <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-1.5">
        Etiqueta
      </p>
      <p className={cn(
        'font-mono font-bold tabular-nums text-3xl tracking-tight leading-none',
        value === 0 && 'text-zinc-300',
        value > 0  && 'text-emerald-600',
        value < 0  && 'text-rose-600',
      )}>
        {display}
      </p>
    </div>
    <span className={cn(
      'text-xs font-semibold px-3 py-1.5 rounded-full',
      value === 0 && 'bg-zinc-100 text-zinc-400',
      value > 0  && 'bg-emerald-100 text-emerald-700',
      value < 0  && 'bg-rose-100 text-rose-700',
    )}>
      {badge}
    </span>
  </div>
</div>
```

---

## Grid de Inputs

Para formularios con 4 campos relacionados, preferir 2×2 sobre lista vertical:

```tsx
<div className="grid grid-cols-2 gap-3">
  {/* campo 1 */}
  {/* campo 2 */}
  {/* campo 3 */}
  {/* campo 4 */}
</div>
```

---

## Resumen de Métodos de Pago

Grid 4 columnas con label muted arriba y monto mono abajo. Separado del total por `border-t border-zinc-200`.

---

## Sheet Pattern

Mismo principio que Modal pero para `SheetContent`:

```tsx
<SheetContent side="right" className="w-full sm:max-w-[540px] p-0 gap-0 flex flex-col">
  <SheetHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
    <SheetTitle className="text-sm font-semibold text-zinc-900 tracking-tight" />
    <SheetDescription className="text-xs text-zinc-400" />
  </SheetHeader>
  {/* Zona de input/filtro */}
  <div className="px-6 py-3 border-b border-zinc-100">...</div>
  {/* Contenido scrollable */}
  <ScrollArea className="flex-1">...</ScrollArea>
</SheetContent>
```

## Lista de Resultados (estilo ledger)

Sin cards individuales. Filas con `border-b border-zinc-100 hover:bg-zinc-50`.
Contador de resultados como label de sección uppercase.

## Result Card — Folio como Hero

```tsx
<button className="w-full text-left px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors">
  {/* Folio — monospace bold, lo primero visible */}
  <span className="font-mono font-bold text-sm tracking-tight text-zinc-900">{folio}</span>
  {/* Estado — pill badge con color semántico del backend */}
  <span
    className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none shrink-0"
    style={{ backgroundColor: `${color}20`, color }}
  >
    {statusName}
  </span>
  {/* Nombre cliente — secundario */}
  <p className="text-xs font-medium text-zinc-800">{name}</p>
  {/* Monto + estado de pago — derecha */}
  <p className="font-mono font-semibold tabular-nums text-sm text-zinc-900">{amount}</p>
  <p className="text-[10px] font-medium text-emerald-600|amber-600|rose-600">{paymentStatus}</p>
</button>
```

## Detail View — Secciones sin Separator

```tsx
{/* NO usar <Separator /> — usar border-b en el div de sección */}
<div className="px-6 py-4 border-b border-zinc-100">
  <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-2.5">
    Sección
  </p>
  {/* datos */}
</div>
```

Labels de campo (solo lectura):
```tsx
<p className="text-[10px] text-zinc-400">Label</p>
<p className="text-xs font-medium text-zinc-800 mt-0.5">Valor</p>
```

## Botones — Reglas

- **CTA principal:** `<Button>` sin className → usa `bg-primary` (azul agua)
- **Secundario:** `<Button variant="outline">`
- **Sutil:** `<Button variant="ghost">`
- **Destructivo (confirmar eliminar):** `<Button className="bg-rose-600 hover:bg-rose-700">`
- **NUNCA** hardcodear `bg-zinc-900 hover:bg-zinc-800 text-white`

## Table Headers — Regla

```tsx
import { TABLE_HEADER_CLASS as TH } from '@/shared/utils/constants';
// No definir TH local en cada archivo
```

## Status Badges — Componente compartido

```tsx
import { StatusBadge } from '@/shared/components/ui/status-badge';
<StatusBadge active={item.isActive} />
// No usar inline span con clases de emerald/zinc cada vez
```

Colores del StatusBadge:
```
Activo:   bg-emerald-50 border border-emerald-100 text-emerald-700  dot: bg-emerald-500
Inactivo: bg-zinc-100   border border-zinc-200    text-zinc-400     dot: bg-zinc-400
```

## Payment Status — Badge consistente

Siempre usar badge con bg+border (no solo texto de color). `text-[10px] font-semibold`:
```
Pagado:    bg-emerald-50 border border-emerald-100 text-emerald-700
Parcial:   bg-amber-50   border border-amber-100   text-amber-700
Pendiente: bg-rose-50    border border-rose-100    text-rose-700
```

## Order Status — Badge dinámico (color del backend)

Sin border. `text-[10px] font-semibold tracking-wide`:
```tsx
<span
  className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none shrink-0"
  style={{ color: status.color, backgroundColor: `${status.color}20` }}
>
  {status.name}
</span>
```

---

## Componentes implementados

- `CashClosingModal` — modal de corte de caja con patron hero semántico
- `OrderSearchSheet` — sheet de búsqueda con vistas search/detail
- `OrderSearchResultCard` — fila ledger con folio como hero
- `OrderDetailView` — detalle con secciones zinc sin Separator
- `OrderPaymentSection` — resumen financiero zinc-50 + historial collapsible
- `OrderItemsTable` — tabla con headers uppercase y montos mono
- `UserFormDialog` — modal con header + contenido scrollable + footer fijo en bg-zinc-50. Formulario con `id="user-form"` + botón submit con `form="user-form" type="submit"`. Sin botones dentro del form.
- `UsersList` — filtros con filled style (`bg-zinc-100 border-2 border-transparent`) y SelectContent con `rounded-xl border border-zinc-200 bg-white shadow-sm`

## Users Module — Estado de rol inline

Badge inline para roles (cuando no se usa StatusBadge genérico):
```
Activo:   bg-emerald-50 border border-emerald-100 text-emerald-700
Inactivo: bg-zinc-100   border border-zinc-200    text-zinc-400
```
