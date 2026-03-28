import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

import type { Permission, CreatePermissionInput, UpdatePermissionInput } from '@/features/users/types/permission';
import {
  getPermissions,
  createPermission,
  updatePermission,
  deletePermission,
} from '@/api/permissions/permissionsService';

interface PermissionsState {
  permissions: Permission[];
  isLoading: boolean;
  error: string | null;
}

interface PermissionsActions {
  fetchPermissions: () => Promise<void>;
  createPermission: (data: CreatePermissionInput) => Promise<Permission | null>;
  updatePermission: (id: number, data: UpdatePermissionInput) => Promise<Permission | null>;
  deletePermission: (id: number) => Promise<boolean>;
  clearError: () => void;
}

type PermissionsStore = PermissionsState & PermissionsActions;

export const usePermissionsStore = create<PermissionsStore>()(
  immer((set) => ({
    permissions: [],
    isLoading: false,
    error: null,

    fetchPermissions: async (): Promise<void> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const permissions = await getPermissions();
        set((state) => {
          state.permissions = permissions;
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar permisos';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
      }
    },

    createPermission: async (data: CreatePermissionInput): Promise<Permission | null> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const perm = await createPermission(data);
        set((state) => {
          state.permissions.push(perm);
          state.isLoading = false;
        });
        return perm;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear permiso';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return null;
      }
    },

    updatePermission: async (id: number, data: UpdatePermissionInput): Promise<Permission | null> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const updated = await updatePermission(id, data);
        set((state) => {
          const index = state.permissions.findIndex((p) => p.id === id);
          if (index !== -1) state.permissions[index] = updated;
          state.isLoading = false;
        });
        return updated;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar permiso';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return null;
      }
    },

    deletePermission: async (id: number): Promise<boolean> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        await deletePermission(id);
        set((state) => {
          state.permissions = state.permissions.filter((p) => p.id !== id);
          state.isLoading = false;
        });
        return true;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar permiso';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return false;
      }
    },

    clearError: (): void => {
      set((state) => {
        state.error = null;
      });
    },
  }))
);
