# Sidebar Redesign — Distribución y Iconos

**Fecha:** 2026-04-01

## Contexto

El sidebar actual tiene los ítems de navegación en un solo grupo sin jerarquía visual, con iconos genéricos que no representan bien el dominio (ej. `Settings` para Servicios). El objetivo es reorganizar los ítems en dos grupos lógicos, actualizar los iconos a unos más representativos, y mover el info del usuario al fondo junto al botón de cerrar sesión.

## Layout resultante

### Expandido

```
┌─────────────────────────┐
│  🔵 Lavandería      [←] │  ← Logo + botón colapso
├─────────────────────────┤
│  OPERACIÓN DIARIA       │  ← label de sección
│   🧾 Nueva Venta        │
│   📦 Órdenes            │
│   💵 Corte de Caja      │
├─────────────────────────┤
│  ADMINISTRACIÓN         │  ← label de sección
│   📊 Dashboard          │
│   👕 Servicios          │
│   👤 Usuarios           │
├─────────────────────────┤
│  [Avatar] Nombre        │  ← UserInfo
│  Rol                    │
│   🚪 Cerrar Sesión      │
└─────────────────────────┘
```

### Colapsado (solo iconos)

```
┌──────┐
│  🔵  │
├──────┤
│  🧾  │
│  📦  │
│  💵  │
├──────┤
│  📊  │
│  👕  │
│  👤  │
├──────┤
│ [Av] │
│  🚪  │
└──────┘
```

- Las etiquetas de sección (`OPERACIÓN DIARIA`, `ADMINISTRACIÓN`) solo se muestran cuando el sidebar está expandido
- Los separadores `border-b` entre grupos se mantienen en ambos estados

## Cambios de iconos

| Ítem | Icono anterior | Icono nuevo |
|------|---------------|-------------|
| Nueva Venta | `ShoppingCart` | `Receipt` |
| Órdenes | `ClipboardList` | `Package` |
| Corte de Caja | `DollarSign` | `Banknote` |
| Dashboard | `LayoutDashboard` | `BarChart2` |
| Servicios | `Settings` | `Shirt` |
| Usuarios | `Users` | `UserCog` |
| Cerrar Sesión | `LogOut` | `LogOut` (sin cambio) |

## Cambios de orden

**Antes:** Dashboard → Órdenes → Nueva Venta → Servicios → Usuarios

**Después:**
- Grupo 1 (Operación): Nueva Venta → Órdenes → Corte de Caja
- Grupo 2 (Administración): Dashboard → Servicios → Usuarios

## Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `src/shared/config/navigation.config.ts` | Reordenar ítems, actualizar iconos, agregar campo `group` |
| `src/shared/components/layout/NavLink.tsx` | Registrar nuevos iconos en el mapa `ICONS` |
| `src/shared/components/layout/Sidebar.tsx` | Renderizar grupos con etiquetas, mover `UserInfo` al fondo |

## Detalle de implementación

### navigation.config.ts

Agregar campo `group` a cada ítem para que `Sidebar.tsx` pueda agruparlos:

```ts
{ path: '/orders/new', label: 'Nueva Venta', icon: 'Receipt',   group: 'operation', requiredPermission: 'orders:create' }
{ path: '/orders',     label: 'Órdenes',     icon: 'Package',   group: 'operation', requiredPermission: 'orders:view'   }
// Corte de Caja se maneja por separado (no es ruta, es acción)
{ path: '/dashboard',  label: 'Dashboard',   icon: 'BarChart2', group: 'admin',     requiredPermission: 'dashboard:view' }
{ path: '/services',   label: 'Servicios',   icon: 'Shirt',     group: 'admin',     requiredPermission: 'services:view'  }
{ path: '/users',      label: 'Usuarios',    icon: 'UserCog',   group: 'admin',     requiredPermission: 'users:view'     }
```

### NavLink.tsx

Agregar al registro `ICONS`:
```ts
Receipt, Package, Banknote, BarChart2, Shirt, UserCog
```

### Sidebar.tsx

- Filtrar ítems por `group === 'operation'` y `group === 'admin'`
- Corte de Caja se sigue manejando como botón de acción (sin ruta), dentro del grupo Operación
- `UserInfo` se mueve al bloque inferior, antes de `Cerrar Sesión`
- Etiquetas de sección con estilo `text-[10px] font-semibold tracking-widest uppercase text-zinc-400`
- En estado colapsado: etiquetas ocultas, solo un separador `border-b`

### NavItem type

Agregar campo opcional `group` al tipo:
```ts
interface NavItem {
  path: string;
  label: string;
  icon: string;
  requiredPermission: string;
  group?: 'operation' | 'admin';
}
```
