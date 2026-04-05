# Diseño: Alta de Prendas y Precios desde el Formulario de Servicio

**Fecha:** 2026-04-05
**Estado:** Aprobado

---

## Problema

Dar de alta un servicio "por pieza" con sus precios requiere navegar por 3 secciones distintas (Tipos de Prenda → Servicios → Precios) y agregar cada precio uno a uno mediante un diálogo repetitivo. Con 15+ prendas por servicio, esto es muy lento y propenso a errores.

**Dolor principal identificado:** repetición excesiva en la carga de precios (1 diálogo por prenda).

---

## Solución

Embeber la sección de prendas y precios directamente dentro del formulario de servicio, para que crear un servicio y configurar todos sus precios sea una sola operación.

---

## Diseño

### 1. Formulario de servicio ampliado

El `Dialog` del formulario de servicio se comporta diferente según el tipo de cobro:

- **PorPeso:** sin cambios. El formulario es igual al actual (`max-w-lg`).
- **PorPieza:** el diálogo se expande a `max-w-2xl` y aparece una sección de **"Prendas y Precios"** debajo de los campos normales.

#### Sección "Prendas y Precios"

- Lista todas las prendas activas del catálogo global (`garmentTypes`).
- Cada fila tiene:
  - **Checkbox** a la izquierda — indica si la prenda está activa para este servicio.
  - **Input de precio** a la derecha — habilitado solo si el checkbox está marcado.
- Al crear un servicio nuevo: todas las prendas aparecen desmarcadas.
- Al editar un servicio existente: las prendas con precio aparecen marcadas y con el precio pre-llenado; las demás aparecen desmarcadas.
- Desmarcar una prenda existente pone `isActive: false` en el `serviceGarment` (no elimina el registro).
- Botón **"Nueva prenda"** abre un mini-modal para capturar nombre + precio. Al guardar:
  1. Crea el tipo de prenda en el catálogo global.
  2. Lo agrega a la lista ya marcado y con precio.

#### Guardar

Un solo "Guardar" envía el servicio completo. El payload incluye **solo las prendas marcadas** (activas). El backend interpreta la lista como la "verdad actual": activa las prendas del listado y desactiva (`isActive: false`) cualquier `serviceGarment` existente para ese servicio que no esté en la lista.

---

### 2. Flujos

#### Crear servicio PorPieza (nuevo)
1. Abrir "Nuevo Servicio".
2. Llenar código, nombre, categoría, tipo de cobro = "PorPieza".
3. Aparece la sección de prendas.
4. Marcar las prendas que aplican y capturar precio de cada una.
5. "Guardar" → crea servicio + todos los precios en una operación.

#### Editar servicio PorPieza (existente)
1. Abrir "Editar Servicio".
2. Las prendas con precio aparecen marcadas y pre-llenadas.
3. El usuario puede agregar prendas nuevas, actualizar precios, o desactivar prendas.
4. "Guardar" → actualiza servicio + precios en batch.

#### Servicio PorPeso
- Sin cambios. La sección de prendas no aparece.

#### Sección "Precios" en el rail lateral
- Sigue existiendo como vista de consulta y edición directa de precios individuales.
- Ya no es el camino principal para el alta inicial de precios.

---

### 3. Backend

#### Extensión de Create/Update Service

Los comandos `CreateServiceCommand` y `UpdateServiceCommand` aceptan un campo opcional:

```csharp
public List<ServiceGarmentPriceInput>? GarmentPrices { get; set; }

public record ServiceGarmentPriceInput(int GarmentTypeId, decimal UnitPrice);
```

Si `GarmentPrices` viene poblado, el command handler llama internamente a la lógica de `AddServicePrice` / `UpdateServicePrice` en batch dentro de la misma transacción.

Si `GarmentPrices` es null o vacío, el comportamiento es idéntico al actual — sin cambios en lógica existente.

#### Endpoints existentes sin cambios
- `POST /api/service-garments` y `PUT /api/service-garments/{id}` siguen funcionando para edición individual desde la sección "Precios".

#### Payload de ejemplo

```json
{
  "code": "PLAN-01",
  "name": "Planchado",
  "chargeType": "PorPieza",
  "categoryId": 2,
  "garmentPrices": [
    { "garmentTypeId": 3, "unitPrice": 45.00 },
    { "garmentTypeId": 7, "unitPrice": 85.00 }
  ]
}
```

---

### 4. Frontend

#### ServicesSection
- Detecta el cambio de `chargeType` a "PorPieza" y renderiza la sección de prendas.
- La lista de prendas se toma del store `garmentTypesStore` (ya cargado en la página).
- Estado local del formulario: array de `{ garmentTypeId, unitPrice, isActive }` para las prendas seleccionadas.

#### servicesStore
- `createService` y `updateService` aceptan `garmentPrices` opcionales en el payload.
- Después del save exitoso, llama a `fetchServiceGarments()` para mantener la sección "Precios" sincronizada.

#### Mini-modal "Nueva prenda"
- Campos: nombre (requerido), precio (requerido).
- Al guardar: llama a `garmentTypesStore.createGarmentType()` **inmediatamente** (API call) para obtener el `id` real. Luego agrega la prenda al estado local del formulario como marcada con ese `id` y el precio capturado.
- Si el usuario abandona el formulario de servicio sin guardar, la prenda queda creada en el catálogo global (comportamiento aceptable — los tipos de prenda son globales y reutilizables).
- Es un componente nuevo pequeño, colocado dentro de `ServicesSection.tsx`.

---

## Archivos a modificar

### Backend
- `CreateServiceCommand.cs` / `CreateServiceCommandHandler.cs`
- `UpdateServiceCommand.cs` / `UpdateServiceCommandHandler.cs`
- `CreateServiceCommandValidator.cs` / `UpdateServiceCommandValidator.cs`

### Frontend
- `Frontend/src/features/services/components/ServicesSection.tsx` — ampliar formulario
- `Frontend/src/features/services/stores/servicesStore.ts` — pasar `garmentPrices` en create/update
- `Frontend/src/api/services/index.ts` — incluir `garmentPrices` en el payload

---

## Lo que NO cambia

- La sección "Precios" del rail lateral — sigue igual.
- Los endpoints de serviceGarments individuales.
- El flujo para servicios PorPeso.
- El catálogo de Tipos de Prenda — sigue siendo global y editable desde su propia sección.
