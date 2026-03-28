import type { Role } from './role';

export interface User {
  id: number;
  username: string;
  fullName: string;
  email: string;
  isActive: boolean;
  role: Role | null;
  createdAt: string;
  lastLogin?: string;
  createdBy?: number;
}

export interface CreateUserInput {
  username: string;
  fullName: string;
  email: string;
  password: string;
  roleId: number | null;
  isActive: boolean;
}

export interface UpdateUserInput {
  fullName?: string;
  email?: string;
  roleId?: number | null;
  isActive?: boolean;
  password?: string;
}

export interface UserFilters {
  search?: string;
  isActive?: boolean;
  roleId?: number;
  sortBy?: 'fullName' | 'username' | 'createdAt' | 'lastLogin';
  sortOrder?: 'asc' | 'desc';
}
