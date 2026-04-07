export interface Permission {
  id: number;
  name: string;
  module: string;
  section: string;
  label: string;
  description: string | null;
}

export interface CreatePermissionInput {
  name: string;
  module: string;
  section: string;
  label: string;
  description?: string;
}

export type UpdatePermissionInput = Partial<CreatePermissionInput>;
