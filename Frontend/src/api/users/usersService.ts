import api from '@/api/axiosConfig';

import type { User, UserFilters, CreateUserInput, UpdateUserInput } from '@/features/users/types/user';

export async function getUsers(filters?: UserFilters): Promise<User[]> {
  const res = await api.get<User[]>('/users', { params: filters });
  return res.data;
}

export async function getUserById(id: number): Promise<User> {
  const res = await api.get<User>(`/users/${id}`);
  return res.data;
}

export async function createUser(data: CreateUserInput): Promise<User> {
  const res = await api.post<User>('/users', data);
  return res.data;
}

export async function updateUser(id: number, data: UpdateUserInput): Promise<User> {
  const res = await api.put<User>(`/users/${id}`, data);
  return res.data;
}

export async function deleteUser(id: number): Promise<void> {
  await api.delete(`/users/${id}`);
}

export async function toggleUserStatus(id: number): Promise<User> {
  const res = await api.patch<User>(`/users/${id}/toggle-status`);
  return res.data;
}
