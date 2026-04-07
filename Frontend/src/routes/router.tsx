import { createBrowserRouter, Navigate, useNavigate } from 'react-router-dom';

import { ProtectedRoute } from './ProtectedRoute';
import { PublicRoute } from './PublicRoute';
import { MainLayout } from '@/layouts/MainLayout';
import { LoginPage } from '@/features/auth/pages/LoginPage';
import { Dashboard } from '@/features/dashboard/pages/Dashboard';
import { OrdersList } from '@/features/orders/pages/OrdersList';
import { NewOrder } from '@/features/orders/pages/NewOrder';
import { UsersList } from '@/features/users/pages/UsersList';
import { ServicesList } from '@/features/services/pages/ServicesList';

function UnauthorizedPage() {
  const navigate = useNavigate();
  return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-4 text-center px-4">
      <p className="text-4xl font-bold text-zinc-900">403</p>
      <p className="text-lg font-medium text-zinc-700">Acceso denegado</p>
      <p className="text-sm text-zinc-400">No tienes permisos para ver esta página.</p>
      <button
        onClick={() => navigate(-1)}
        className="mt-2 text-xs text-zinc-500 underline underline-offset-2"
      >
        Volver
      </button>
    </div>
  );
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/login" replace />,
  },
  {
    element: <PublicRoute />,
    children: [
      {
        path: '/login',
        element: <LoginPage />,
      },
    ],
  },
  {
    element: <MainLayout />,
    children: [
      {
        element: <ProtectedRoute requiredPermission="dashboard.general:view" />,
        children: [{ path: '/dashboard', element: <Dashboard /> }],
      },
      {
        element: <ProtectedRoute requiredPermission="orders.lista:view" />,
        children: [{ path: '/orders', element: <OrdersList /> }],
      },
      {
        element: <ProtectedRoute requiredPermission="orders.nueva:create" />,
        children: [{ path: '/orders/new', element: <NewOrder /> }],
      },
      {
        element: <ProtectedRoute requiredPermissions={[
          'services.servicios:view',
          'services.categorias:view',
          'services.prendas:view',
          'services.precios:view',
          'services.descuentos:view',
        ]} />,
        children: [{ path: '/services', element: <ServicesList /> }],
      },
      {
        element: <ProtectedRoute requiredPermissions={[
          'users.usuarios:view',
          'users.roles:view',
        ]} />,
        children: [
          { path: '/users', element: <UsersList /> },
        ],
      },
    ],
  },
  {
    path: '/unauthorized',
    element: <UnauthorizedPage />,
  },
  {
    path: '*',
    element: <Navigate to="/login" replace />,
  },
]);
