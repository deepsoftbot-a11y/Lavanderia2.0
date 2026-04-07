/**
 * Typed permission constants — format: "module.section:action"
 * Mirrors the permission catalog stored in the database.
 * Use these constants instead of raw strings to avoid typos.
 *
 * Example:
 *   <PermissionGuard permission={PERMS.ORDERS.LISTA.VIEW}>
 *   hasPermission(PERMS.USERS.ROLES.MANAGE)
 */
export const PERMS = {
  DASHBOARD: {
    GENERAL: {
      VIEW: 'dashboard.general:view',
    },
  },
  ORDERS: {
    LISTA: {
      VIEW: 'orders.lista:view',
      EXPORT: 'orders.lista:export',
    },
    NUEVA: {
      CREATE: 'orders.nueva:create',
    },
    DETALLE: {
      VIEW: 'orders.detalle:view',
      EDIT: 'orders.detalle:edit',
      PAY: 'orders.detalle:pay',
    },
    CORTE: {
      MANAGE: 'orders.corte:manage',
    },
  },
  SERVICES: {
    SERVICIOS: {
      VIEW: 'services.servicios:view',
      CREATE: 'services.servicios:create',
      EDIT: 'services.servicios:edit',
      DELETE: 'services.servicios:delete',
    },
    CATEGORIAS: {
      VIEW: 'services.categorias:view',
      CREATE: 'services.categorias:create',
      EDIT: 'services.categorias:edit',
      DELETE: 'services.categorias:delete',
    },
    PRENDAS: {
      VIEW: 'services.prendas:view',
      CREATE: 'services.prendas:create',
      EDIT: 'services.prendas:edit',
      DELETE: 'services.prendas:delete',
    },
    PRECIOS: {
      VIEW: 'services.precios:view',
      CREATE: 'services.precios:create',
      EDIT: 'services.precios:edit',
      DELETE: 'services.precios:delete',
    },
    DESCUENTOS: {
      VIEW: 'services.descuentos:view',
      CREATE: 'services.descuentos:create',
      EDIT: 'services.descuentos:edit',
      DELETE: 'services.descuentos:delete',
    },
  },
  USERS: {
    USUARIOS: {
      VIEW: 'users.usuarios:view',
      CREATE: 'users.usuarios:create',
      EDIT: 'users.usuarios:edit',
      DELETE: 'users.usuarios:delete',
      TOGGLE: 'users.usuarios:toggle',
    },
    ROLES: {
      VIEW: 'users.roles:view',
      CREATE: 'users.roles:create',
      EDIT: 'users.roles:edit',
      DELETE: 'users.roles:delete',
    },
  },
} as const;
