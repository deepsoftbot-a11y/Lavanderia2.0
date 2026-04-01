import type { NavigationConfig } from '@/shared/types/navigation';

export const NAVIGATION_CONFIG: NavigationConfig = {
  appName: 'Lavandería',
  items: [
    // Operación diaria
    {
      path: '/orders/new',
      label: 'Nueva Venta',
      icon: 'Receipt',
      requiredPermission: 'orders:create',
      group: 'operation',
    },
    {
      path: '/orders',
      label: 'Órdenes',
      icon: 'Package',
      requiredPermission: 'orders:view',
      group: 'operation',
    },
    // Administración
    {
      path: '/dashboard',
      label: 'Dashboard',
      icon: 'BarChart2',
      requiredPermission: 'dashboard:view',
      group: 'admin',
    },
    {
      path: '/services',
      label: 'Servicios',
      icon: 'Shirt',
      requiredPermission: 'services:view',
      group: 'admin',
    },
    {
      path: '/users',
      label: 'Usuarios',
      icon: 'UserCog',
      requiredPermission: 'users:view',
      group: 'admin',
    },
  ],
};
