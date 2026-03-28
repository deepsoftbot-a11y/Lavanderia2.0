export interface Permission {
  id: number;
  name: string;
  module: string;
  description: string | null;
}

export interface CreatePermissionInput {
  name: string;
  module: string;
  description?: string;
}

export type UpdatePermissionInput = Partial<CreatePermissionInput>;
