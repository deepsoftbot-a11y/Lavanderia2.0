import type { NavigationConfig } from '@/shared/types/navigation';

export const NAVIGATION_CONFIG: NavigationConfig = {
  appName: 'Lavandería',
  items: [
    {
      path: '/dashboard',
      label: 'Dashboard',
      icon: 'LayoutDashboard',
      requiredPermission: 'dashboard:view',
    },
    {
      path: '/orders',
      label: 'Órdenes',
      icon: 'ClipboardList',
      requiredPermission: 'orders:view',
    },
    {
      path: '/orders/new',
      label: 'Nueva Venta',
      icon: 'ShoppingCart',
      requiredPermission: 'orders:create',
    },
    {
      path: '/services',
      label: 'Servicios',
      icon: 'Settings',
      requiredPermission: 'services:view',
    },
    {
      path: '/users',
      label: 'Usuarios',
      icon: 'Users',
      requiredPermission: 'users:view',
    },
  ],
};
