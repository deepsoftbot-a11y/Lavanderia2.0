// Active modules with pages in the application
export const MODULES = {
  DASHBOARD: 'dashboard',
  ORDERS: 'orders',
  SERVICES: 'services',
  USERS: 'users',
} as const;

export type Module = (typeof MODULES)[keyof typeof MODULES];

// Permission key format: "module.section:action"
export type PermissionKey =
  // Dashboard
  | 'dashboard.general:view'
  // Orders
  | 'orders.lista:view'
  | 'orders.lista:export'
  | 'orders.nueva:create'
  | 'orders.detalle:view'
  | 'orders.detalle:edit'
  | 'orders.detalle:pay'
  | 'orders.corte:manage'
  // Services — servicios
  | 'services.servicios:view'
  | 'services.servicios:create'
  | 'services.servicios:edit'
  | 'services.servicios:delete'
  // Services — categorias
  | 'services.categorias:view'
  | 'services.categorias:create'
  | 'services.categorias:edit'
  | 'services.categorias:delete'
  // Services — prendas
  | 'services.prendas:view'
  | 'services.prendas:create'
  | 'services.prendas:edit'
  | 'services.prendas:delete'
  // Services — precios
  | 'services.precios:view'
  | 'services.precios:create'
  | 'services.precios:edit'
  | 'services.precios:delete'
  // Services — descuentos
  | 'services.descuentos:view'
  | 'services.descuentos:create'
  | 'services.descuentos:edit'
  | 'services.descuentos:delete'
  // Users — usuarios
  | 'users.usuarios:view'
  | 'users.usuarios:create'
  | 'users.usuarios:edit'
  | 'users.usuarios:delete'
  | 'users.usuarios:toggle'
  // Users — roles
  | 'users.roles:view'
  | 'users.roles:create'
  | 'users.roles:edit'
  | 'users.roles:delete';

// Permission map for efficient lookups
export type PermissionMap = Record<string, boolean>;
