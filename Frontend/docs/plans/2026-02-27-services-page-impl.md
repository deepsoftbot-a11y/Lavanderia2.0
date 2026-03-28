# Plan de Implementación: Rediseño Página de Servicios

**Fecha:** 2026-02-27
**Diseño base:** `2026-02-27-services-page-redesign.md`

---

## Orden de implementación

```
1. ServicesRail            — componente de navegación lateral
2. CategoriesSection       — entidad nueva, sin UI previa
3. GarmentTypesSection     — refactor de GarmentsTab.tsx
4. PricesSection           — refactor de PricesTab.tsx (tabla plana + filtro)
5. DiscountsSection        — refactor de DiscountsTab.tsx
6. ServicesSection         — refactor de ServicesTab.tsx
7. ServicesList.tsx        — reescribir con nuevo layout rail + panel
8. Limpieza                — eliminar archivos *Tab.tsx reemplazados
```

---

## Paso 1 — ServicesRail

**Archivo:** `src/features/services/components/ServicesRail.tsx`

```typescript
type ServiceSection = 'services' | 'categories' | 'garmentTypes' | 'prices' | 'discounts';

interface ServicesRailProps {
  active: ServiceSection;
  onChange: (section: ServiceSection) => void;
}
```

**Estructura del rail:**
- Grupos con label uppercase + items
- Item activo: `border-l-2 border-zinc-900 bg-zinc-50 text-zinc-900 font-medium`
- Item inactivo: `text-zinc-500 hover:text-zinc-800 hover:bg-zinc-50`
- Sin íconos — texto puro (estilo settings profesional)

**Grupos:**
```
CATÁLOGO        → Servicios, Categorías
PRENDAS         → Tipos de Prenda, Precios
COMERCIAL       → Descuentos
```

**Responsive:** En `< lg`, render como `<Select>` con las 5 opciones.

---

## Paso 2 — CategoriesSection

**Archivo:** `src/features/services/components/CategoriesSection.tsx`

Entidad nueva que no tenía UI propia. Usa `useCategoriesStore`.

**Columnas de tabla:** Nombre · Descripción · Estado · Acciones

**Form fields (create):**
- `name` — Input, required, min 2 / max 100
- `description` — Textarea, opcional

**Form fields (edit, además):**
- `isActive` — Checkbox

**Schema:** `createCategorySchema` / `updateCategorySchema`
**Store:** `useCategoriesStore` → `createCategory`, `updateCategory`, `deleteCategory`
**Modal size:** `max-w-sm`

---

## Paso 3 — GarmentTypesSection

**Archivo:** `src/features/services/components/GarmentTypesSection.tsx`

Refactor directo de `GarmentsTab.tsx`. Cambios:
- Renombrar archivo
- Ajustar nombre de componente exportado: `GarmentTypesSection`
- Sin cambios de lógica

---

## Paso 4 — PricesSection

**Archivo:** `src/features/services/components/PricesSection.tsx`

Refactor de `PricesTab.tsx`. Cambio clave: **tabla plana** en vez de secciones por servicio.

**Columnas:** Servicio · Tipo de Prenda · Precio Unitario · Estado · Acciones

**Filtro opcional encima de tabla:**
```tsx
<Select value={filterServiceId} onValueChange={setFilterServiceId}>
  <SelectItem value="all">Todos los servicios</SelectItem>
  {pieceServices.map(s => <SelectItem value={s.id.toString()}>{s.name}</SelectItem>)}
</Select>
```

**Datos:** `serviceGarments` filtrados por `serviceId` si hay filtro activo.

**Form alta:**
- Select servicio (ByPiece only, activos)
- Select tipo de prenda (activos, excluir los ya configurados para ese servicio)
- Input precio unitario

**Form edición:** Solo precio unitario + checkbox isActive.

**Componente:** Mantener `PriceFormDialog` como subcomponente interno.

---

## Paso 5 — DiscountsSection

**Archivo:** `src/features/services/components/DiscountsSection.tsx`

Refactor directo de `DiscountsTab.tsx`. Cambios:
- Renombrar archivo y componente exportado
- Sin cambios de lógica (ya actualizado en sesión anterior)

---

## Paso 6 — ServicesSection

**Archivo:** `src/features/services/components/ServicesSection.tsx`

Refactor directo de `ServicesTab.tsx`. Cambios:
- Renombrar archivo y componente exportado
- Sin cambios de lógica (ya actualizado en sesión anterior)

---

## Paso 7 — ServicesList.tsx (reescritura)

**Archivo:** `src/features/services/pages/ServicesList.tsx`

```typescript
type ServiceSection = 'services' | 'categories' | 'garmentTypes' | 'prices' | 'discounts';

export function ServicesList() {
  const [activeSection, setActiveSection] = useState<ServiceSection>('services');

  // Inicializar todos los stores
  useEffect(() => {
    fetchServices();
    fetchCategories();
    fetchGarmentTypes();
    fetchServiceGarments();
    fetchDiscounts();
  }, [...]);

  return (
    <div className="flex flex-col h-full">
      {/* Page header */}
      <div className="px-6 py-5 border-b border-zinc-100">
        <h1>Gestión de Servicios</h1>
      </div>

      {/* Rail + Panel */}
      <div className="flex flex-1 overflow-hidden">
        {/* Rail — desktop */}
        <div className="hidden lg:block w-[220px] border-r border-zinc-200 shrink-0 overflow-y-auto">
          <ServicesRail active={activeSection} onChange={setActiveSection} />
        </div>

        {/* Selector — mobile */}
        <div className="lg:hidden px-4 py-3 border-b border-zinc-100">
          {/* Select con las 5 opciones */}
        </div>

        {/* Panel */}
        <div className="flex-1 overflow-y-auto">
          {activeSection === 'services'     && <ServicesSection />}
          {activeSection === 'categories'   && <CategoriesSection />}
          {activeSection === 'garmentTypes' && <GarmentTypesSection />}
          {activeSection === 'prices'       && <PricesSection />}
          {activeSection === 'discounts'    && <DiscountsSection />}
        </div>
      </div>
    </div>
  );
}
```

---

## Paso 8 — Limpieza

Eliminar archivos reemplazados:
- `src/features/services/components/ServicesTab.tsx`
- `src/features/services/components/GarmentsTab.tsx`
- `src/features/services/components/PricesTab.tsx`
- `src/features/services/components/DiscountsTab.tsx`

---

## Verificación

```bash
npm run build   # 0 errores TypeScript
npm run lint    # sin errores nuevos
```

Revisar:
- [ ] Rail selecciona sección correctamente
- [ ] CRUD completo en cada sección (crear, editar, eliminar, listar)
- [ ] PricesSection muestra tabla plana + filtro por servicio funciona
- [ ] CategoriesSection es funcional (es nueva)
- [ ] Responsive: mobile muestra Select en lugar de rail
- [ ] Stores inicializados correctamente en ServicesList
