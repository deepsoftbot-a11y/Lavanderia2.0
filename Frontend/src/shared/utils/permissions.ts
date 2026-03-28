export function hasPermission(permissions: string[], permission: string): boolean {
  return permissions.includes(permission);
}

export function hasAnyPermission(permissions: string[], required: string[]): boolean {
  return required.some((p) => permissions.includes(p));
}

export function hasAllPermissions(permissions: string[], required: string[]): boolean {
  return required.every((p) => permissions.includes(p));
}

export function permissionsToMap(permissions: string[]): Record<string, boolean> {
  return permissions.reduce<Record<string, boolean>>((acc, p) => {
    acc[p] = true;
    return acc;
  }, {});
}

export function getModulePermissions(permissions: string[], module: string): string[] {
  return permissions.filter((p) => p.startsWith(`${module}.`) || p.startsWith(`${module}:`));
}

export function canAccessModule(permissions: string[], module: string): boolean {
  return getModulePermissions(permissions, module).length > 0;
}
