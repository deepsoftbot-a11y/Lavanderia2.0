# Servicios: Alta de Prendas y Precios en Batch — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Embeber la sección de prendas y precios directamente en el formulario de servicio (PorPieza), reemplazando el flujo actual de N diálogos separados por un único Submit.

**Architecture:** Backend extiende `CreateServiceCommand` / `UpdateServiceCommand` con un array opcional `GarmentPrices`. Los handlers llaman la lógica de dominio existente (`AddPriceForGarment`, `UpdatePriceForGarment`, `DeactivatePriceById`) en batch antes del save. Frontend amplía `ServicesSection.tsx` con un sub-componente inline `GarmentPricesSubsection` y un mini-modal para crear prendas nuevas sobre la marcha.

**Tech Stack:** C# / ASP.NET Core 8 / FluentValidation / MediatR — React 19 / TypeScript / Zod / React Hook Form / Zustand + Immer / Tailwind / Shadcn

---

## Contexto clave

- `ServiceGarmentId` en el dominio = `TipoPrendaId` en la BD = `garmentTypeId` en el frontend.
- El mapper `ServiceMapper.ToInfrastructure` ya maneja precios con `Id.IsEmpty` (nuevos = sin ID), por lo que agregar precios al agregado antes de `UpdateAsync` funciona sin cambios en el repositorio.
- El `UpdateAsync` del repositorio elimina físicamente de la BD los `ServiciosPrenda` cuyo `ServicioPrendaId` no aparezca en el agregado. Para desactivar en lugar de borrar, se debe llamar `price.Deactivate()` en el dominio — así el precio permanece en `_prices` con `IsActive = false`, y el repositorio lo actualiza (no lo borra).
- El frontend ya llama `fetchServiceGarments()` en `ServicesList.tsx` al montar — necesitamos re-llamarlo después de un create/update con precios para mantener la sección "Precios" sincronizada.

---

## Task 1: Backend — Extender CreateServiceCommand con GarmentPrices

**Files:**
- Modify: `Backend/src/LaundryManagement.Application/Commands/Services/CreateServiceCommand.cs`
- Modify: `Backend/src/LaundryManagement.Application/Commands/Services/CreateServiceCommandHandler.cs`
- Modify: `Backend/src/LaundryManagement.Application/Commands/Services/CreateServiceCommandValidator.cs`

### Paso 1: Agregar el record `ServiceGarmentPriceInput` y el campo al command

Editar `CreateServiceCommand.cs` — agregar al final del archivo, dentro del namespace:

```csharp
// Al final de CreateServiceCommand.cs, dentro del namespace
public sealed record ServiceGarmentPriceInput
{
    public int GarmentTypeId { get; init; }
    public decimal UnitPrice { get; init; }
}
```

Y en `CreateServiceCommand`, agregar la propiedad:

```csharp
public List<ServiceGarmentPriceInput>? GarmentPrices { get; init; }
```

### Paso 2: Actualizar el handler para procesar precios en batch

En `CreateServiceCommandHandler.Handle`, después de crear el agregado `service` (antes de `_serviceRepository.AddAsync`) y solo si `chargeType == "piece"`, agregar:

```csharp
// Agregar precios en batch (solo para servicios por pieza)
if (command.GarmentPrices != null && command.GarmentPrices.Count > 0)
{
    foreach (var gp in command.GarmentPrices)
    {
        service.AddPriceForGarment(
            ServiceGarmentId.From(gp.GarmentTypeId),
            Money.FromDecimal(gp.UnitPrice)
        );
    }
}
```

Esto va ANTES de la llamada a `_serviceRepository.AddAsync(service, cancellationToken)`. El repositorio ya sabe manejar precios en el agregado al crear.

### Paso 3: Agregar validación para GarmentPrices

En `CreateServiceCommandValidator.cs`:

```csharp
When(x => x.GarmentPrices != null && x.GarmentPrices.Count > 0, () =>
{
    RuleForEach(x => x.GarmentPrices)
        .ChildRules(gp =>
        {
            gp.RuleFor(x => x.GarmentTypeId)
                .GreaterThan(0).WithMessage("El ID del tipo de prenda debe ser válido");
            gp.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("El precio unitario debe ser mayor que cero");
        });
});
```

### Paso 4: Verificar que el build compila

```bash
cd Backend
dotnet build LaundryManagement.sln
```

Esperado: sin errores de compilación.

### Paso 5: Commit

```bash
git add Backend/src/LaundryManagement.Application/Commands/Services/CreateServiceCommand.cs
git add Backend/src/LaundryManagement.Application/Commands/Services/CreateServiceCommandHandler.cs
git add Backend/src/LaundryManagement.Application/Commands/Services/CreateServiceCommandValidator.cs
git commit -m "feat(backend): extender CreateServiceCommand con GarmentPrices batch"
```

---

## Task 2: Backend — Extender UpdateServiceCommand con GarmentPrices

**Files:**
- Modify: `Backend/src/LaundryManagement.Application/Commands/Services/UpdateServiceCommand.cs`
- Modify: `Backend/src/LaundryManagement.Application/Commands/Services/UpdateServiceCommandHandler.cs`
- Modify: `Backend/src/LaundryManagement.Application/Commands/Services/UpdateServiceCommandValidator.cs`

### Paso 1: Agregar campo al command

En `UpdateServiceCommand.cs`, agregar:

```csharp
/// <summary>
/// Lista de precios por prenda. Solo aplica para servicios PorPieza.
/// Si se provee, se sincroniza como "verdad actual": activa los incluidos,
/// desactiva los que ya existían pero no están en la lista.
/// Si es null, los precios existentes no se modifican.
/// </summary>
public List<ServiceGarmentPriceInput>? GarmentPrices { get; init; }
```

El type `ServiceGarmentPriceInput` ya existe en `CreateServiceCommand.cs` dentro del mismo namespace — no necesita redefinirse.

### Paso 2: Actualizar el handler para sincronizar precios

En `UpdateServiceCommandHandler.Handle`, después de `service.UpdateInfo(...)` y antes de `_serviceRepository.UpdateAsync(service, cancellationToken)`, agregar:

```csharp
// Sincronizar precios en batch (solo si se proveyó la lista y el servicio es por pieza)
if (command.GarmentPrices != null && service.IsPieceBased)
{
    // IDs de garment types que deben quedar activos
    var incomingGarmentIds = command.GarmentPrices
        .Select(gp => gp.GarmentTypeId)
        .ToHashSet();

    // Desactivar precios existentes que ya no están en la lista
    foreach (var existingPrice in service.Prices.Where(p => p.IsActive))
    {
        if (!incomingGarmentIds.Contains(existingPrice.ServiceGarmentId.Value))
        {
            service.DeactivatePriceById(existingPrice.Id);
        }
    }

    // Agregar o actualizar precios de la lista entrante
    foreach (var gp in command.GarmentPrices)
    {
        var garmentId = ServiceGarmentId.From(gp.GarmentTypeId);
        var price = Money.FromDecimal(gp.UnitPrice);
        var existingActive = service.Prices
            .FirstOrDefault(p => p.ServiceGarmentId == garmentId && p.IsActive);

        if (existingActive != null)
        {
            service.UpdatePriceForGarment(garmentId, price);
        }
        else
        {
            // Puede existir uno inactivo; AddPriceForGarment ya verifica solo activos
            service.AddPriceForGarment(garmentId, price);
        }
    }
}
```

### Paso 3: Agregar validación

En `UpdateServiceCommandValidator.cs`, agregar (igual que en Create):

```csharp
When(x => x.GarmentPrices != null && x.GarmentPrices.Count > 0, () =>
{
    RuleForEach(x => x.GarmentPrices)
        .ChildRules(gp =>
        {
            gp.RuleFor(x => x.GarmentTypeId)
                .GreaterThan(0).WithMessage("El ID del tipo de prenda debe ser válido");
            gp.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("El precio unitario debe ser mayor que cero");
        });
});
```

### Paso 4: Build

```bash
cd Backend
dotnet build LaundryManagement.sln
```

Esperado: sin errores.

### Paso 5: Commit

```bash
git add Backend/src/LaundryManagement.Application/Commands/Services/UpdateServiceCommand.cs
git add Backend/src/LaundryManagement.Application/Commands/Services/UpdateServiceCommandHandler.cs
git add Backend/src/LaundryManagement.Application/Commands/Services/UpdateServiceCommandValidator.cs
git commit -m "feat(backend): extender UpdateServiceCommand con GarmentPrices batch sync"
```

---

## Task 3: Frontend — Tipos y API

**Files:**
- Modify: `Frontend/src/features/services/types/service.ts`
- Modify: `Frontend/src/api/services/servicesService.api.ts`

### Paso 1: Agregar `GarmentPriceInput` a los tipos

En `service.ts`, agregar antes de `CreateServiceInput`:

```typescript
export interface GarmentPriceInput {
  garmentTypeId: number;
  unitPrice: number;
}
```

Y en `CreateServiceInput` y `UpdateServiceInput`, agregar el campo opcional:

```typescript
export interface CreateServiceInput {
  categoryId: number;
  code: string;
  name: string;
  chargeType: ChargeType;
  pricePerKg?: number;
  minWeight?: number;
  maxWeight?: number;
  description?: string;
  estimatedTime?: number;
  garmentPrices?: GarmentPriceInput[];  // NUEVO
}

export interface UpdateServiceInput extends Partial<CreateServiceInput> {
  isActive?: boolean;
  garmentPrices?: GarmentPriceInput[];  // NUEVO (ya heredado, pero explícito)
}
```

### Paso 2: Actualizar la API — mapear garmentPrices al payload

En `servicesService.api.ts`, el `createService` ya pasa `...input` al backend. Como el campo se llama `garmentPrices` en el frontend y el backend espera `garmentPrices` (camelCase, ASP.NET lo deserializa igual), no hay conversión adicional necesaria.

Verificar que en `createService`:
```typescript
const payload = input.chargeType
  ? { ...input, chargeType: toApi(input.chargeType) }
  : input;
```
El spread ya incluye `garmentPrices` si está presente — no se necesita cambio.

Lo mismo para `updateService`. Verificar que el spread incluye `garmentPrices`. No se necesita cambio de código en la API si ya hace spread.

### Paso 3: Commit

```bash
git add Frontend/src/features/services/types/service.ts
git commit -m "feat(frontend): agregar GarmentPriceInput a tipos de servicio"
```

---

## Task 4: Frontend — Store: refetch de serviceGarments tras save

**Files:**
- Modify: `Frontend/src/features/services/stores/servicesStore.ts`

### Paso 1: Importar `fetchServiceGarments`

El store de `servicesStore` no tiene acceso directo a `serviceGarmentsStore`. La forma limpia es recibir el callback como parámetro en `createService` / `updateService`, o llamarlo desde el componente después del save.

**Decisión: llamarlo desde el componente** (`ServicesSection.tsx`) después del await exitoso — es más simple y evita acoplar los stores. No se necesita cambiar el store.

Este task queda documentado pero la acción real ocurre en Task 7 (componente).

---

## Task 5: Frontend — Componente `GarmentPricesSubsection`

**Files:**
- Create: `Frontend/src/features/services/components/GarmentPricesSubsection.tsx`

Este componente recibe la lista de tipos de prenda del catálogo global y el estado actual de prendas seleccionadas, y renderiza la tabla de checkboxes + inputs de precio.

```tsx
import { CurrencyInput } from '@/shared/components/ui/currency-input';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Label } from '@/shared/components/ui/label';
import type { GarmentType } from '@/features/services/types/garmentType';
import type { GarmentPriceInput } from '@/features/services/types/service';

interface GarmentPricesSubsectionProps {
  garmentTypes: GarmentType[];
  value: GarmentPriceInput[];
  onChange: (prices: GarmentPriceInput[]) => void;
}

export function GarmentPricesSubsection({
  garmentTypes,
  value,
  onChange,
}: GarmentPricesSubsectionProps) {
  const getEntry = (id: number) => value.find((p) => p.garmentTypeId === id);
  const isChecked = (id: number) => !!getEntry(id);

  const handleCheck = (garmentTypeId: number, checked: boolean) => {
    if (checked) {
      onChange([...value, { garmentTypeId, unitPrice: 0 }]);
    } else {
      onChange(value.filter((p) => p.garmentTypeId !== garmentTypeId));
    }
  };

  const handlePrice = (garmentTypeId: number, unitPrice: number | '') => {
    onChange(
      value.map((p) =>
        p.garmentTypeId === garmentTypeId
          ? { ...p, unitPrice: typeof unitPrice === 'number' ? unitPrice : 0 }
          : p
      )
    );
  };

  if (garmentTypes.length === 0) {
    return (
      <p className="text-xs text-zinc-400 italic">
        No hay tipos de prenda definidos. Créalos desde la sección "Tipos de Prenda".
      </p>
    );
  }

  return (
    <div className="space-y-1 max-h-60 overflow-y-auto pr-1">
      {garmentTypes
        .filter((gt) => gt.isActive)
        .map((gt) => {
          const checked = isChecked(gt.id);
          const entry = getEntry(gt.id);
          return (
            <div
              key={gt.id}
              className="flex items-center gap-3 rounded-lg px-2 py-1.5 hover:bg-zinc-50 transition-colors"
            >
              <Checkbox
                id={`gp-${gt.id}`}
                checked={checked}
                onCheckedChange={(v) => handleCheck(gt.id, !!v)}
              />
              <Label
                htmlFor={`gp-${gt.id}`}
                className="flex-1 text-sm text-zinc-700 cursor-pointer select-none"
              >
                {gt.name}
              </Label>
              <div className="w-32">
                <CurrencyInput
                  value={checked ? (entry?.unitPrice ?? '') : ''}
                  onChange={(v) => handlePrice(gt.id, v)}
                  disabled={!checked}
                  placeholder="—"
                />
              </div>
            </div>
          );
        })}
    </div>
  );
}
```

### Paso 2: Verificar que TypeScript compila

```bash
cd Frontend
npm run build 2>&1 | head -30
```

Esperado: sin errores de tipo en el nuevo componente.

### Paso 3: Commit

```bash
git add Frontend/src/features/services/components/GarmentPricesSubsection.tsx
git commit -m "feat(frontend): agregar GarmentPricesSubsection — tabla checkbox+precio por prenda"
```

---

## Task 6: Frontend — Mini-modal `NewGarmentDialog`

**Files:**
- Create: `Frontend/src/features/services/components/NewGarmentDialog.tsx`

Este diálogo crea una prenda nueva (API call inmediato) y la devuelve al padre para agregarla a la lista.

```tsx
import { useState } from 'react';
import { Loader2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { CurrencyInput } from '@/shared/components/ui/currency-input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useToast } from '@/shared/hooks/use-toast';
import type { GarmentPriceInput } from '@/features/services/types/service';

interface NewGarmentDialogProps {
  open: boolean;
  onClose: () => void;
  onCreated: (entry: GarmentPriceInput) => void;
}

export function NewGarmentDialog({ open, onClose, onCreated }: NewGarmentDialogProps) {
  const { createGarmentType } = useGarmentTypesStore();
  const { toast } = useToast();
  const [name, setName] = useState('');
  const [price, setPrice] = useState<number | ''>('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const canSave = name.trim().length > 0 && typeof price === 'number' && price > 0 && !isSubmitting;

  const handleSave = async () => {
    if (!canSave) return;
    setIsSubmitting(true);
    try {
      const newGarment = await createGarmentType({ name: name.trim(), description: '' });
      if (!newGarment) throw new Error('No se pudo crear el tipo de prenda');
      onCreated({ garmentTypeId: newGarment.id, unitPrice: price as number });
      toast({ title: `Prenda "${name.trim()}" creada y agregada` });
      setName('');
      setPrice('');
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Error al crear la prenda';
      toast({ title: 'Error', description: message, variant: 'destructive' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(o) => { if (!o) onClose(); }}>
      <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            Nueva prenda
          </DialogTitle>
        </DialogHeader>
        <div className="px-6 py-4 space-y-4">
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Nombre *</Label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ej: Camisa, Edredón..."
              autoFocus
            />
          </div>
          <div className="space-y-1">
            <Label className="text-xs text-zinc-500 font-medium">Precio unitario *</Label>
            <CurrencyInput value={price} onChange={setPrice} />
          </div>
        </div>
        <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
          <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancelar
          </Button>
          <Button onClick={handleSave} disabled={!canSave}>
            {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Guardar
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
```

**Nota:** `createGarmentType` en el store devuelve `GarmentType | null`. Verificar la firma actual en `garmentTypesStore.ts` antes de implementar. Si devuelve void, ajustar para que retorne el objeto creado (ver notas en Tarea 6b abajo).

### Paso 6b: Verificar/ajustar `garmentTypesStore` si `createGarmentType` no retorna el objeto

Leer `Frontend/src/features/services/stores/garmentTypesStore.ts`. Si `createGarmentType` no retorna el `GarmentType` creado, modificarlo para que lo retorne:

```typescript
createGarmentType: async (input) => {
  // ...
  const garmentType = await garmentTypesApi.createGarmentType(input);
  set((state) => { state.garmentTypes.push(garmentType); ... });
  return garmentType;  // ← asegurarse que retorna el objeto
},
```

### Paso 7: Commit

```bash
git add Frontend/src/features/services/components/NewGarmentDialog.tsx
git add Frontend/src/features/services/stores/garmentTypesStore.ts  # si se modificó
git commit -m "feat(frontend): agregar NewGarmentDialog — crear prenda + precio inline"
```

---

## Task 7: Frontend — Integrar en `ServicesSection.tsx`

**Files:**
- Modify: `Frontend/src/features/services/components/ServicesSection.tsx`

Este es el task más largo. Integra los componentes anteriores y actualiza el formulario de servicio.

### Paso 1: Agregar imports

En `ServicesSection.tsx`, agregar imports:

```typescript
import { Plus as PlusSmall } from 'lucide-react'; // ya existe Plus, usar para el botón de prenda
import { GarmentPricesSubsection } from './GarmentPricesSubsection';
import { NewGarmentDialog } from './NewGarmentDialog';
import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useServiceGarmentsStore } from '@/features/services/stores/serviceGarmentsStore';
import type { GarmentPriceInput } from '@/features/services/types/service';
```

### Paso 2: Actualizar `ServiceFormContent`

La interfaz de props ya recibe `service?: Service`. Necesita recibir también los tipos de prenda y el estado inicial de precios. La forma más limpia: el estado de `garmentPrices` vive dentro del form como estado local (no en react-hook-form, ya que es una lista dinámica).

Añadir a `ServiceFormContentProps`:
```typescript
initialGarmentPrices?: GarmentPriceInput[];
```

Dentro de `ServiceFormContent`, agregar:

```typescript
const { garmentTypes } = useGarmentTypesStore();
const [garmentPrices, setGarmentPrices] = useState<GarmentPriceInput[]>(
  initialGarmentPrices ?? []
);
const [newGarmentOpen, setNewGarmentOpen] = useState(false);
const chargeType = watch('chargeType');
```

Modificar el `handleSubmit` del form para incluir `garmentPrices`:
```typescript
// El onSubmit ya recibe data de react-hook-form.
// Pasarlo al padre junto con garmentPrices:
<form onSubmit={handleSubmit((data) => onSubmit({ ...data, garmentPrices }))} ...>
```

### Paso 3: Expandir el dialog cuando es PorPieza

Cambiar la sección del diálogo del form para que cuando `chargeType === 'PorPieza'` y hay tipos de prenda, se muestre `GarmentPricesSubsection`.

Agregar dentro del form, después del campo `Tiempo estimado`:

```tsx
{chargeType === CHARGE_TYPE.PorPieza && (
  <div className="space-y-2 pt-2 border-t border-zinc-100">
    <div className="flex items-center justify-between">
      <Label className="text-xs text-zinc-500 font-medium">Prendas y Precios</Label>
      <button
        type="button"
        onClick={() => setNewGarmentOpen(true)}
        className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-700 font-medium"
      >
        <PlusSmall className="h-3 w-3" />
        Nueva prenda
      </button>
    </div>
    <GarmentPricesSubsection
      garmentTypes={garmentTypes}
      value={garmentPrices}
      onChange={setGarmentPrices}
    />
    <NewGarmentDialog
      open={newGarmentOpen}
      onClose={() => setNewGarmentOpen(false)}
      onCreated={(entry) => setGarmentPrices((prev) => [...prev, entry])}
    />
  </div>
)}
```

### Paso 4: Expandir el dialog a `max-w-2xl` cuando es PorPieza

En `ServicesSection`, el `Dialog` tiene `max-w-lg`. Necesita saber el tipo de cobro actual para cambiar el tamaño. Solución: levantar el estado de `chargeType` al componente padre o simplemente usar `max-w-2xl` siempre (el form se ve bien en ambos casos).

Opción más simple: cambiar a `max-w-2xl` siempre:
```tsx
<DialogContent className="max-w-2xl p-0 gap-0 overflow-hidden">
```

### Paso 5: Inicializar `garmentPrices` al editar un servicio

En `ServicesSection.handleOpenEdit`, al setear `selectedService`, también calcular los `initialGarmentPrices` desde `serviceGarments`:

```typescript
const { serviceGarments } = useServiceGarmentsStore();

const getInitialGarmentPrices = (service: Service): GarmentPriceInput[] => {
  return serviceGarments
    .filter((sg) => sg.serviceId === service.id && sg.isActive)
    .map((sg) => ({ garmentTypeId: sg.garmentTypeId, unitPrice: sg.unitPrice }));
};
```

Pasar `initialGarmentPrices={selectedService ? getInitialGarmentPrices(selectedService) : []}` a `ServiceFormContent`.

### Paso 6: Actualizar `handleSubmit` en `ServicesSection` para pasar `garmentPrices` y hacer refetch

```typescript
const { fetchServiceGarments } = useServiceGarmentsStore();

const handleSubmit = async (data: Record<string, unknown>) => {
  setIsSubmitting(true);
  try {
    const { garmentPrices, ...serviceData } = data as any;
    if (selectedService) {
      await updateService(selectedService.id, { ...serviceData, garmentPrices });
    } else {
      await createService({ ...serviceData, garmentPrices });
    }
    // Refrescar la sección "Precios" del rail
    await fetchServiceGarments();
    toast({ title: selectedService ? 'Servicio actualizado' : 'Servicio creado' });
    setDialogOpen(false);
  } catch (err) {
    const message = err instanceof Error ? err.message : 'No se pudo guardar el servicio';
    toast({ title: 'Error', description: message, variant: 'destructive' });
  } finally {
    setIsSubmitting(false);
  }
};
```

### Paso 7: Build y verificar tipos

```bash
cd Frontend
npm run build 2>&1 | head -50
```

Esperado: sin errores de TypeScript.

### Paso 8: Commit

```bash
git add Frontend/src/features/services/components/ServicesSection.tsx
git commit -m "feat(frontend): embeber GarmentPricesSubsection en formulario de servicio PorPieza"
```

---

## Task 8: Prueba manual end-to-end

### Flujo 1 — Crear servicio PorPieza con precios

1. Levantar backend: `cd Backend/src/LaundryManagement.API && dotnet run`
2. Levantar frontend: `cd Frontend && npm run dev`
3. Ir a Gestión de Servicios → Servicios → "Nuevo Servicio"
4. Llenar código, nombre, categoría, cambiar tipo de cobro a "PorPieza"
5. Verificar que aparece la sección "Prendas y Precios" con los tipos de prenda del catálogo
6. Marcar 3+ prendas y asignar precios
7. Click "Guardar"
8. Verificar: el servicio aparece en la tabla
9. Ir a la sección "Precios" del rail → verificar que los precios aparecen correctamente

### Flujo 2 — Editar servicio existente

1. Editar el servicio recién creado
2. Verificar que las prendas marcadas aparecen con sus precios pre-llenados
3. Agregar una prenda más y cambiar el precio de otra
4. Guardar y verificar cambios en la sección "Precios"

### Flujo 3 — Nueva prenda inline

1. Abrir el formulario de un servicio PorPieza
2. Click "Nueva prenda"
3. Ingresar nombre y precio
4. Guardar → verificar que la prenda aparece en la lista marcada
5. Guardar el servicio → verificar que la prenda quedó en el catálogo global

### Flujo 4 — Servicio PorPeso (no debe cambiar)

1. Crear/editar un servicio PorPeso
2. Verificar que la sección de prendas NO aparece
3. Verificar que el flujo existente funciona igual

### Paso final: Commit de smoke test (si se hacen anotaciones)

```bash
git add -p  # solo si hay cambios de ajuste
git commit -m "fix: ajustes post prueba manual servicios prendas precios"
```

---

## Notas de implementación

- El `ServiceGarmentId` en el dominio C# es el `TipoPrendaId` de la BD, que corresponde al `garmentTypeId` del frontend. No confundir con el `ServicioPrendaId` (ID de la relación servicio-prenda).
- Si `GarmentPrices` es `null` en el command, los handlers no modifican los precios existentes — retrocompatible con los endpoints de serviceGarments individuales.
- El repositorio `UpdateAsync` en Infrastructure puede borrar físicamente registros de `ServiciosPrenda` si su ID no aparece en el agregado. Llamar `DeactivatePriceById` en lugar de eliminarlos del agregado garantiza que permanecen con `Activo = false`.
- El componente `GarmentPricesSubsection` filtra solo `garmentTypes.isActive` para no mostrar prendas desactivadas en el catálogo.
