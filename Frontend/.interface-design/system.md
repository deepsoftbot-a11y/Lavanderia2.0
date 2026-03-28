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

Botón primario:    bg-zinc-900 hover:bg-zinc-800 text-white
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
      <Button className="bg-zinc-900 hover:bg-zinc-800 text-white">Confirmar</Button>
    </div>
  </div>
</DialogContent>
```

---

## Input Monetario (CurrencyInputField)

Prefijo `$` absoluto + input con `pl-7 font-mono tabular-nums text-right`.

```tsx
<div className="relative">
  <span className="absolute left-3 top-1/2 -translate-y-1/2 text-zinc-400 text-sm font-mono select-none pointer-events-none">
    $
  </span>
  <Input
    type="number"
    step="0.01"
    className="pl-7 font-mono tabular-nums text-right"
  />
</div>
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

## Componentes implementados

- `CashClosingModal` — modal de corte de caja con patron hero semántico
- `OrderSearchSheet` — sheet de búsqueda con vistas search/detail
- `OrderSearchResultCard` — fila ledger con folio como hero
- `OrderDetailView` — detalle con secciones zinc sin Separator
- `OrderPaymentSection` — resumen financiero zinc-50 + historial collapsible
- `OrderItemsTable` — tabla con headers uppercase y montos mono
