import type { Permission } from './permission';

export interface Role {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  permissions?: Permission[];
}

export interface CreateRoleInput {
  name: string;
  description?: string;
  isActive: boolean;
  permissionIds: number[];
}

export type UpdateRoleInput = Partial<CreateRoleInput>;
