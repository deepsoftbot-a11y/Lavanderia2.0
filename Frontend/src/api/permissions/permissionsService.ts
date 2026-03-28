import api from '@/api/axiosConfig';

import type { Permission, CreatePermissionInput, UpdatePermissionInput } from '@/features/users/types/permission';

export async function getPermissions(): Promise<Permission[]> {
  const res = await api.get<Permission[]>('/permissions');
  return res.data;
}

export async function createPermission(data: CreatePermissionInput): Promise<Permission> {
  const res = await api.post<Permission>('/permissions', data);
  return res.data;
}

export async function updatePermission(id: number, data: UpdatePermissionInput): Promise<Permission> {
  const res = await api.put<Permission>(`/permissions/${id}`, data);
  return res.data;
}

export async function deletePermission(id: number): Promise<void> {
  await api.delete(`/permissions/${id}`);
}
