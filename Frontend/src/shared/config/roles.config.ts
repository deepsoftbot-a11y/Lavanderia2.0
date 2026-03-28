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
  'dashboard:view',
  'dashboard:export',
  'orders:view',
  'orders:create',
  'orders:edit',
  'orders:delete',
  'orders:export',
  'customers:view',
  'customers:create',
  'customers:edit',
  'customers:delete',
  'customers:export',
  'services:view',
  'services:manage',
  'inventory:view',
  'inventory:manage',
  'users:view',
  'users:create',
  'users:edit',
  'users:delete',
  'users:manage',
  'reports:view',
  'reports:export',
  'settings:view',
  'settings:manage',
];

// Empleado has limited permissions
const EMPLEADO_PERMISSIONS: PermissionKey[] = [
  'orders:view',
  'orders:create',
  'orders:edit',
  'customers:view',
  'customers:create',
  'customers:edit',
  'services:view',
  'inventory:view',
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
