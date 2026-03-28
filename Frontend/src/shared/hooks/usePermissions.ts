import { useMemo } from 'react';

import { useAuthStore } from '@/features/auth/stores/authStore';
import {
  hasPermission,
  hasAnyPermission,
  hasAllPermissions,
  permissionsToMap,
  canAccessModule,
  getModulePermissions,
} from '@/shared/utils/permissions';

export function usePermissions() {
  const user = useAuthStore((state) => state.user);
  const permissions = useAuthStore((state) => state.permissions);

  const permissionMap = useMemo(() => {
    return permissionsToMap(permissions);
  }, [permissions]);

  return {
    user,
    permissions,
    permissionMap,
    hasPermission: (permission: string) => hasPermission(permissions, permission),
    hasAnyPermission: (required: string[]) => hasAnyPermission(permissions, required),
    hasAllPermissions: (required: string[]) => hasAllPermissions(permissions, required),
    canAccessModule: (module: string) => canAccessModule(permissions, module),
    getModulePermissions: (module: string) => getModulePermissions(permissions, module),
  };
}
