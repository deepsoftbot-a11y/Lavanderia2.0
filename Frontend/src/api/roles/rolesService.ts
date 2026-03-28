import api from '@/api/axiosConfig';

import type { Role, CreateRoleInput, UpdateRoleInput } from '@/features/users/types/role';

export async function getRoles(): Promise<Role[]> {
  const res = await api.get<Role[]>('/roles');
  return res.data;
}

export async function getRoleWithPermissions(id: number): Promise<Role> {
  const res = await api.get<Role>(`/roles/${id}`);
  return res.data;
}

export async function createRole(data: CreateRoleInput): Promise<Role> {
  const res = await api.post<Role>('/roles', data);
  return res.data;
}

export async function updateRole(id: number, data: UpdateRoleInput): Promise<Role> {
  const res = await api.put<Role>(`/roles/${id}`, data);
  return res.data;
}

export async function deleteRole(id: number): Promise<void> {
  await api.delete(`/roles/${id}`);
}

export async function toggleRoleStatus(id: number): Promise<Role> {
  const res = await api.patch<Role>(`/roles/${id}/toggle-status`);
  return res.data;
}
