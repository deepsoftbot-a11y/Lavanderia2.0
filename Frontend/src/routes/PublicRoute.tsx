import { Navigate, Outlet } from 'react-router-dom';

import { useAuthStore } from '@/features/auth/stores/authStore';

export function PublicRoute() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const permissions = useAuthStore((state) => state.permissions);

  if (isAuthenticated) {
    const redirectPath = permissions.includes('dashboard.general:view')
      ? '/dashboard'
      : permissions.includes('orders.lista:view')
      ? '/orders'
      : '/orders/new';
    return <Navigate to={redirectPath} replace />;
  }

  return <Outlet />;
}
