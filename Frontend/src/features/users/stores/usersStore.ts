import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

import type { User, UserFilters, CreateUserInput, UpdateUserInput } from '@/features/users/types/user';
import {
  getUsers,
  getUserById,
  createUser,
  updateUser,
  deleteUser,
  toggleUserStatus,
} from '@/api/users';

interface UsersState {
  users: User[];
  selectedUser: User | null;
  isLoading: boolean;
  error: string | null;
  filters: UserFilters;
}

interface UsersActions {
  fetchUsers: () => Promise<void>;
  fetchUserById: (id: number) => Promise<void>;
  createUser: (input: CreateUserInput) => Promise<User | null>;
  updateUser: (id: number, input: UpdateUserInput) => Promise<User | null>;
  deleteUser: (id: number) => Promise<boolean>;
  toggleUserStatus: (id: number) => Promise<boolean>;
  setFilters: (filters: Partial<UserFilters>) => void;
  clearFilters: () => void;
  setSelectedUser: (user: User | null) => void;
  clearError: () => void;
}

type UsersStore = UsersState & UsersActions;

const initialFilters: UserFilters = {
  search: '',
  sortBy: 'fullName',
  sortOrder: 'asc',
};

export const useUsersStore = create<UsersStore>()(
  immer((set, get) => ({
    users: [],
    selectedUser: null,
    isLoading: false,
    error: null,
    filters: initialFilters,

    fetchUsers: async (): Promise<void> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });

      try {
        const filters = get().filters;
        const users = await getUsers(filters);
        set((state) => {
          state.users = users;
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar usuarios';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
      }
    },

    fetchUserById: async (id: number): Promise<void> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });

      try {
        const user = await getUserById(id);
        set((state) => {
          state.selectedUser = user;
          state.isLoading = false;
        });
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cargar usuario';
        set((state) => {
          state.error = message;
          state.isLoading = false;
          state.selectedUser = null;
        });
      }
    },

    createUser: async (input: CreateUserInput): Promise<User | null> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });

      try {
        const newUser = await createUser(input);
        set((state) => {
          state.users.push(newUser);
          state.isLoading = false;
        });
        return newUser;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al crear usuario';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return null;
      }
    },

    updateUser: async (id: number, input: UpdateUserInput): Promise<User | null> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });

      try {
        const updatedUser = await updateUser(id, input);
        set((state) => {
          const index = state.users.findIndex((u) => u.id === id);
          if (index !== -1) {
            state.users[index] = updatedUser;
          }
          if (state.selectedUser?.id === id) {
            state.selectedUser = updatedUser;
          }
          state.isLoading = false;
        });
        return updatedUser;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al actualizar usuario';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return null;
      }
    },

    deleteUser: async (id: number): Promise<boolean> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });

      try {
        await deleteUser(id);
        set((state) => {
          state.users = state.users.filter((u) => u.id !== id);
          if (state.selectedUser?.id === id) {
            state.selectedUser = null;
          }
          state.isLoading = false;
        });
        return true;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al eliminar usuario';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return false;
      }
    },

    toggleUserStatus: async (id: number): Promise<boolean> => {
      set((state) => {
        state.error = null;
      });

      try {
        const updatedUser = await toggleUserStatus(id);
        set((state) => {
          const index = state.users.findIndex((u) => u.id === id);
          if (index !== -1) {
            state.users[index] = updatedUser;
          }
          if (state.selectedUser?.id === id) {
            state.selectedUser = updatedUser;
          }
        });
        return true;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al cambiar estado';
        set((state) => {
          state.error = message;
        });
        return false;
      }
    },

    setFilters: (filters: Partial<UserFilters>): void => {
      set((state) => {
        state.filters = { ...state.filters, ...filters };
      });
      get().fetchUsers();
    },

    clearFilters: (): void => {
      set((state) => {
        state.filters = initialFilters;
      });
      get().fetchUsers();
    },

    setSelectedUser: (user: User | null): void => {
      set((state) => {
        state.selectedUser = user;
      });
    },

    clearError: (): void => {
      set((state) => {
        state.error = null;
      });
    },
  }))
);
