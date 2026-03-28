// Permission action types
export const PERMISSION_ACTIONS = {
  VIEW: 'view',
  CREATE: 'create',
  EDIT: 'edit',
  DELETE: 'delete',
  EXPORT: 'export',
  MANAGE: 'manage',
} as const;

export type PermissionAction = (typeof PERMISSION_ACTIONS)[keyof typeof PERMISSION_ACTIONS];

// Application modules
export const MODULES = {
  DASHBOARD: 'dashboard',
  ORDERS: 'orders',
  CUSTOMERS: 'customers',
  SERVICES: 'services',
  INVENTORY: 'inventory',
  USERS: 'users',
  REPORTS: 'reports',
  SETTINGS: 'settings',
} as const;

export type Module = (typeof MODULES)[keyof typeof MODULES];

// Permission identifier format: "module:action"
export type PermissionKey = `${Module}:${PermissionAction}`;

// Permission definition
export interface Permission {
  key: PermissionKey;
  module: Module;
  action: PermissionAction;
  label: string;
  description: string;
}

// Permission map for efficient lookups
export type PermissionMap = Record<PermissionKey, boolean>;

// Module permissions grouping
export interface ModulePermissions {
  module: Module;
  label: string;
  permissions: Permission[];
}
