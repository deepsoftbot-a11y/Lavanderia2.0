import type { PermissionKey } from '@/shared/types/permission';

export interface NavItem {
  path: string;
  label: string;
  icon: string;
  requiredPermission: PermissionKey;
}

export interface NavigationConfig {
  appName: string;
  items: NavItem[];
}
