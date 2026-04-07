// DEPRECATED: Los roles son ahora datos dinámicos cargados desde la API (tabla Roles).
// Este archivo se mantiene como referencia de la configuración inicial de roles.
// No usar en código nuevo — usar useRolesStore() en su lugar.
import type { PermissionKey } from '@/shared/types/permission';
import { USER_ROLES } from '@/features/auth/types/auth';

interface RoleConfig {
  role: string;
  label: string;
  description: string;
  permissions: PermissionKey[];
  isSystem: boolean;
}

// Admin has ALL permissions
const ADMIN_PERMISSIONS: PermissionKey[] = [
  'dashboard.general:view',
  'orders.lista:view',
  'orders.lista:export',
  'orders.nueva:create',
  'orders.detalle:view',
  'orders.detalle:edit',
  'orders.detalle:pay',
  'orders.corte:manage',
  'services.servicios:view',
  'services.servicios:create',
  'services.servicios:edit',
  'services.servicios:delete',
  'services.categorias:view',
  'services.categorias:create',
  'services.categorias:edit',
  'services.categorias:delete',
  'services.prendas:view',
  'services.prendas:create',
  'services.prendas:edit',
  'services.prendas:delete',
  'services.precios:view',
  'services.precios:create',
  'services.precios:edit',
  'services.precios:delete',
  'services.descuentos:view',
  'services.descuentos:create',
  'services.descuentos:edit',
  'services.descuentos:delete',
  'users.usuarios:view',
  'users.usuarios:create',
  'users.usuarios:edit',
  'users.usuarios:delete',
  'users.usuarios:toggle',
  'users.roles:view',
  'users.roles:create',
  'users.roles:edit',
  'users.roles:delete',
];

// Empleado has limited permissions (operational)
const EMPLEADO_PERMISSIONS: PermissionKey[] = [
  'orders.lista:view',
  'orders.lista:export',
  'orders.nueva:create',
  'orders.detalle:view',
  'orders.detalle:edit',
  'orders.detalle:pay',
  'orders.corte:manage',
  'services.servicios:view',
  'services.categorias:view',
  'services.prendas:view',
  'services.precios:view',
  'services.descuentos:view',
];

// Role configurations
export const ROLE_CONFIGS: RoleConfig[] = [
  {
    role: USER_ROLES.ADMIN,
    label: 'Administrador',
    description: 'Acceso completo a todas las funcionalidades del sistema',
    permissions: ADMIN_PERMISSIONS,
    isSystem: true,
  },
  {
    role: USER_ROLES.EMPLEADO,
    label: 'Empleado',
    description: 'Acceso limitado para operaciones diarias',
    permissions: EMPLEADO_PERMISSIONS,
    isSystem: true,
  },
];

// Role lookup map
export const ROLE_CONFIG_MAP: Record<string, RoleConfig> = ROLE_CONFIGS.reduce(
  (acc, config) => {
    acc[config.role] = config;
    return acc;
  },
  {} as Record<string, RoleConfig>
);

// Get default permissions for a role
export function getDefaultPermissionsForRole(role: string): PermissionKey[] {
  return ROLE_CONFIG_MAP[role]?.permissions ?? [];
}

// Get role config
export function getRoleConfig(role: string): RoleConfig | undefined {
  return ROLE_CONFIG_MAP[role];
}
