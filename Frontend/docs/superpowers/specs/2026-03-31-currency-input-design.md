quitalo del git# CurrencyInput — Diseño

**Fecha:** 2026-03-31
**Estado:** Aprobado

---

## Contexto

El proyecto usa `NumericInput` con `prefix="$"` para todos los campos de moneda. Tiene dos problemas:

1. Usa `type="number"` — muestra spinners del navegador y acepta caracteres inválidos (`e`, `+`, `-`)
2. No formatea el valor visualmente — el usuario ve `1234.5` en lugar de `1,234.50`

## Solución

Nuevo componente `CurrencyInput` en `src/shared/components/ui/currency-input.tsx` que reemplaza todos los usos de `<NumericInput prefix="$">`.

---

## API

```tsx
interface CurrencyInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value' | 'type'> {
  value: number | '';
  onChange: (value: number | '') => void;
  onBlur?: () => void;
  max?: number;
  hasError?: boolean;
}
```

- `value` / `onChange`: mismo contrato que `NumericInput` — migración drop-in
- `hasError`: estado de error rose, igual que `FieldInput` y `SelectTrigger`
- `max`: límite superior opcional (ej. saldo pendiente en pagos)
- No expone `min` (siempre ≥ 0), `step` (siempre 0.01), ni `prefix` (siempre `$`)
- `placeholder` por defecto: `"0.00"`

---

## Comportamiento

### onChange (mientras escribe)
1. Filtrar caracteres inválidos — aceptar solo dígitos y un único punto
2. Limitar a 2 dígitos después del punto
3. Aplicar separadores de miles a la parte entera: `1234.5` → `1,234.5`
4. Si el string termina en punto (`"1234."`) no llamar `onChange` — esperar más dígitos
5. Si queda vacío → `onChange('')`
6. En cualquier otro caso → `onChange(parseFloat(rawValue))`

### onKeyDown
Bloquear `e`, `E`, `+`, `-` antes de que lleguen al input.

### onFocus
Mantener el formato con comas — el usuario sigue editando con el valor formateado visible.

### onBlur
- Con valor: formatear a exactamente 2 decimales → `"1,234.5"` → `"1,234.50"`
- Vacío: quedarse vacío, llamar `onBlur?.()` del caller

### Sincronización con valor externo
Usar `isFocused` ref (igual que `NumericInput`): solo actualizar el display desde el prop `value` cuando el campo no tiene foco. Previene que RHF sobreescriba lo que el usuario está escribiendo.

---

## Estilo

Mismo sistema que `FieldInput` — sin tokens nuevos:

| Estado | Clases |
|--------|--------|
| Normal | `border-2 border-blue-300 text-indigo-900 placeholder:text-blue-300` |
| Hover | `hover:border-blue-400` |
| Focus | `focus:border-blue-600` |
| Error | `border-rose-400 text-rose-900 placeholder:text-rose-400` |
| Disabled | `opacity-50 cursor-not-allowed` |

**Prefijo `$`:** span absoluto izquierda, `text-zinc-400` (normal) / `text-rose-400` (error). Input con `pl-7`.

**Valor:** `text-right font-mono tabular-nums` — alineado a la derecha, tipografía monoespaciada.

**Forma:** `h-11 rounded-2xl` — idéntico al resto de inputs del proyecto.

---

## Migración

Reemplazar `<NumericInput prefix="$" ...>` por `<CurrencyInput ...>` en 6 archivos:

| Archivo | Campos |
|---------|--------|
| `CashClosingModal.tsx` | Fondo Inicial, Efectivo Contado, Ajuste, Retiro |
| `PaymentForm.tsx` | Monto del pago |
| `PricesSection.tsx` | Precio unitario (crear y editar) |
| `ServicesSection.tsx` | Precio / kg |

**Props que se eliminan** en cada uso (ya las asume el componente):
- `prefix="$"`
- `step={0.01}`
- `className="text-right"`
- `min={0}`

**`NumericInput` permanece** para casos no-monetarios: porcentajes (`DiscountsSection`), piezas y kilos (`AddServiceDialog`, `CartItem`).

---

## Archivos afectados

**Nuevo:**
- `src/shared/components/ui/currency-input.tsx`

**Modificados:**
- `src/features/orders/components/cashClosing/CashClosingModal.tsx`
- `src/features/orders/components/payments/PaymentForm.tsx`
- `src/features/services/components/PricesSection.tsx`
- `src/features/services/components/ServicesSection.tsx`
