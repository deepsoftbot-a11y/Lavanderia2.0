// DEPRECATED: Los permisos son ahora datos dinámicos cargados desde la API (tabla Permisos).
// Este archivo se mantiene como referencia de los NombrePermiso válidos del sistema.
// No usar en código nuevo — usar usePermissionsStore() en su lugar.
import type { Permission, ModulePermissions, PermissionKey } from '@/shared/types/permission';
import { MODULES, PERMISSION_ACTIONS } from '@/shared/types/permission';

// Helper to create permission definition
function createPermission(
  module: string,
  action: string,
  label: string,
  description: string
): Permission {
  return {
    key: `${module}:${action}` as PermissionKey,
    module: module as any,
    action: action as any,
    label,
    description,
  };
}

// Dashboard permissions
const DASHBOARD_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.DASHBOARD,
    PERMISSION_ACTIONS.VIEW,
    'Ver Dashboard',
    'Permite visualizar el panel de control y estadísticas'
  ),
  createPermission(
    MODULES.DASHBOARD,
    PERMISSION_ACTIONS.EXPORT,
    'Exportar Dashboard',
    'Permite exportar reportes del dashboard'
  ),
];

// Orders permissions
const ORDERS_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.ORDERS,
    PERMISSION_ACTIONS.VIEW,
    'Ver Órdenes',
    'Permite visualizar la lista de órdenes'
  ),
  createPermission(
    MODULES.ORDERS,
    PERMISSION_ACTIONS.CREATE,
    'Crear Órdenes',
    'Permite crear nuevas órdenes de lavado'
  ),
  createPermission(
    MODULES.ORDERS,
    PERMISSION_ACTIONS.EDIT,
    'Editar Órdenes',
    'Permite modificar órdenes existentes'
  ),
  createPermission(
    MODULES.ORDERS,
    PERMISSION_ACTIONS.DELETE,
    'Eliminar Órdenes',
    'Permite cancelar o eliminar órdenes'
  ),
  createPermission(
    MODULES.ORDERS,
    PERMISSION_ACTIONS.EXPORT,
    'Exportar Órdenes',
    'Permite exportar listado de órdenes'
  ),
];

// Customers permissions
const CUSTOMERS_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.CUSTOMERS,
    PERMISSION_ACTIONS.VIEW,
    'Ver Clientes',
    'Permite visualizar la lista de clientes'
  ),
  createPermission(
    MODULES.CUSTOMERS,
    PERMISSION_ACTIONS.CREATE,
    'Crear Clientes',
    'Permite registrar nuevos clientes'
  ),
  createPermission(
    MODULES.CUSTOMERS,
    PERMISSION_ACTIONS.EDIT,
    'Editar Clientes',
    'Permite modificar información de clientes'
  ),
  createPermission(
    MODULES.CUSTOMERS,
    PERMISSION_ACTIONS.DELETE,
    'Eliminar Clientes',
    'Permite eliminar clientes del sistema'
  ),
  createPermission(
    MODULES.CUSTOMERS,
    PERMISSION_ACTIONS.EXPORT,
    'Exportar Clientes',
    'Permite exportar listado de clientes'
  ),
];

// Services permissions
const SERVICES_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.SERVICES,
    PERMISSION_ACTIONS.VIEW,
    'Ver Servicios',
    'Permite visualizar el catálogo de servicios'
  ),
  createPermission(
    MODULES.SERVICES,
    PERMISSION_ACTIONS.MANAGE,
    'Gestionar Servicios',
    'Permite crear, modificar y eliminar servicios'
  ),
];

// Inventory permissions
const INVENTORY_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.INVENTORY,
    PERMISSION_ACTIONS.VIEW,
    'Ver Inventario',
    'Permite visualizar el inventario de productos'
  ),
  createPermission(
    MODULES.INVENTORY,
    PERMISSION_ACTIONS.MANAGE,
    'Gestionar Inventario',
    'Permite administrar stock y movimientos de inventario'
  ),
];

// Users permissions
const USERS_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.USERS,
    PERMISSION_ACTIONS.VIEW,
    'Ver Usuarios',
    'Permite visualizar la lista de usuarios del sistema'
  ),
  createPermission(
    MODULES.USERS,
    PERMISSION_ACTIONS.CREATE,
    'Crear Usuarios',
    'Permite crear nuevos usuarios del sistema'
  ),
  createPermission(
    MODULES.USERS,
    PERMISSION_ACTIONS.EDIT,
    'Editar Usuarios',
    'Permite modificar información de usuarios'
  ),
  createPermission(
    MODULES.USERS,
    PERMISSION_ACTIONS.DELETE,
    'Eliminar Usuarios',
    'Permite desactivar o eliminar usuarios'
  ),
  createPermission(
    MODULES.USERS,
    PERMISSION_ACTIONS.MANAGE,
    'Gestionar Permisos',
    'Permite asignar y modificar permisos de usuarios'
  ),
];

// Reports permissions
const REPORTS_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.REPORTS,
    PERMISSION_ACTIONS.VIEW,
    'Ver Reportes',
    'Permite visualizar reportes y análisis'
  ),
  createPermission(
    MODULES.REPORTS,
    PERMISSION_ACTIONS.EXPORT,
    'Exportar Reportes',
    'Permite exportar reportes en diferentes formatos'
  ),
];

// Settings permissions
const SETTINGS_PERMISSIONS: Permission[] = [
  createPermission(
    MODULES.SETTINGS,
    PERMISSION_ACTIONS.VIEW,
    'Ver Configuración',
    'Permite visualizar la configuración del sistema'
  ),
  createPermission(
    MODULES.SETTINGS,
    PERMISSION_ACTIONS.MANAGE,
    'Gestionar Configuración',
    'Permite modificar la configuración del sistema'
  ),
];

// Module permissions grouping
export const MODULE_PERMISSIONS: ModulePermissions[] = [
  {
    module: MODULES.DASHBOARD,
    label: 'Panel de Control',
    permissions: DASHBOARD_PERMISSIONS,
  },
  {
    module: MODULES.ORDERS,
    label: 'Órdenes',
    permissions: ORDERS_PERMISSIONS,
  },
  {
    module: MODULES.CUSTOMERS,
    label: 'Clientes',
    permissions: CUSTOMERS_PERMISSIONS,
  },
  {
    module: MODULES.SERVICES,
    label: 'Servicios',
    permissions: SERVICES_PERMISSIONS,
  },
  {
    module: MODULES.INVENTORY,
    label: 'Inventario',
    permissions: INVENTORY_PERMISSIONS,
  },
  {
    module: MODULES.USERS,
    label: 'Usuarios',
    permissions: USERS_PERMISSIONS,
  },
  {
    module: MODULES.REPORTS,
    label: 'Reportes',
    permissions: REPORTS_PERMISSIONS,
  },
  {
    module: MODULES.SETTINGS,
    label: 'Configuración',
    permissions: SETTINGS_PERMISSIONS,
  },
];

// All permissions flattened
export const ALL_PERMISSIONS: Permission[] = MODULE_PERMISSIONS.flatMap(
  (mp) => mp.permissions
);

// Permission lookup map
export const PERMISSION_MAP: Record<PermissionKey, Permission> =
  ALL_PERMISSIONS.reduce((acc, permission) => {
    acc[permission.key] = permission;
    return acc;
  }, {} as Record<PermissionKey, Permission>);
