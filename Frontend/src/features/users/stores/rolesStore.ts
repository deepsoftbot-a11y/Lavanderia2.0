import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

import type { Role, CreateRoleInput, UpdateRoleInput } from '@/features/users/types/role';
import {
  getRoles,
  getRoleWithPermissions,
  createRole,
  updateRole,
  deleteRole,
  toggleRoleStatus,
} from '@/api/roles/rolesService';

interface RolesState {
  roles: Role[];
  selectedRole: Role | null;
  isLoading: boolean;
  error: string | null;
}

interface RolesActions {
  fetchRoles: () => Promise<void>;
  fetchRoleWithPermissions: (id: number) => Promise<void>;
  createRole: (data: CreateRoleInput) => Promise<Role | null>;
  updateRole: (id: number, data: UpdateRoleInput) => Promise<Role | null>;
  deleteRole: (id: number) => Promise<boolean>;
  toggleRoleStatus: (id: number) => Promise<boolean>;
  setSelectedRole: (role: Role | null) => void;
  clearError: () => void;
}

type RolesStore = RolesState & RolesActions;

export const useRolesStore = create<RolesStore>()(
  immer((set) => ({
    roles: [],
    selectedRole: null,
    isLoading: false,
    error: null,

    fetchRoles: async (): Promise<void> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const roles = await getRoles();
        set((state) => {
          state.roles = roles;
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar roles';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
      }
    },

    fetchRoleWithPermissions: async (id: number): Promise<void> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const role = await getRoleWithPermissions(id);
        set((state) => {
          state.selectedRole = role;
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar rol';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
      }
    },

    createRole: async (data: CreateRoleInput): Promise<Role | null> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const role = await createRole(data);
        set((state) => {
          state.roles.push(role);
          state.isLoading = false;
        });
        return role;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear rol';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return null;
      }
    },

    updateRole: async (id: number, data: UpdateRoleInput): Promise<Role | null> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        const updated = await updateRole(id, data);
        set((state) => {
          const index = state.roles.findIndex((r) => r.id === id);
          if (index !== -1) state.roles[index] = updated;
          if (state.selectedRole?.id === id) state.selectedRole = updated;
          state.isLoading = false;
        });
        return updated;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar rol';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return null;
      }
    },

    deleteRole: async (id: number): Promise<boolean> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });
      try {
        await deleteRole(id);
        set((state) => {
          state.roles = state.roles.filter((r) => r.id !== id);
          if (state.selectedRole?.id === id) state.selectedRole = null;
          state.isLoading = false;
        });
        return true;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar rol';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return false;
      }
    },

    toggleRoleStatus: async (id: number): Promise<boolean> => {
      set((state) => {
        state.error = null;
      });
      try {
        const updated = await toggleRoleStatus(id);
        set((state) => {
          const index = state.roles.findIndex((r) => r.id === id);
          if (index !== -1) state.roles[index] = updated;
          if (state.selectedRole?.id === id) state.selectedRole = updated;
        });
        return true;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cambiar estado del rol';
        set((state) => {
          state.error = message;
        });
        return false;
      }
    },

    setSelectedRole: (role: Role | null): void => {
      set((state) => {
        state.selectedRole = role;
      });
    },

    clearError: (): void => {
      set((state) => {
        state.error = null;
      });
    },
  }))
);
