import { Navigate, Outlet } from 'react-router-dom';

import { useAuthStore } from '@/features/auth/stores/authStore';
import { hasPermission, hasAnyPermission, hasAllPermissions } from '@/shared/utils/permissions';
import type { PermissionKey } from '@/shared/types/permission';

interface ProtectedRouteProps {
  requiredPermission?: PermissionKey;
  requiredPermissions?: PermissionKey[];
  requireAllPermissions?: boolean;
}

export function ProtectedRoute({
  requiredPermission,
  requiredPermissions,
  requireAllPermissions = false,
}: ProtectedRouteProps) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const permissions = useAuthStore((state) => state.permissions);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredPermission && !hasPermission(permissions, requiredPermission)) {
    return <Navigate to="/unauthorized" replace />;
  }

  if (requiredPermissions) {
    const hasAccess = requireAllPermissions
      ? hasAllPermissions(permissions, requiredPermissions)
      : hasAnyPermission(permissions, requiredPermissions);

    if (!hasAccess) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return <Outlet />;
}
