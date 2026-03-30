import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

import { AUTH_CONFIG } from '@/shared/config/auth.config';
import { login as loginApi, validateToken, logout as logoutApi } from '@/api/auth';
import type { User, LoginCredentials } from '@/features/auth/types/auth';

const PERMISSIONS_KEY = 'auth_permissions';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  permissions: string[];
}

interface AuthActions {
  login: (credentials: LoginCredentials) => Promise<boolean>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
  clearError: () => void;
}

type AuthStore = AuthState & AuthActions;

const getStoredToken = (): string | null => {
  return localStorage.getItem(AUTH_CONFIG.tokenKey);
};

// Verifica localmente si el JWT ya expiró, sin hacer una llamada al API.
// Evita requests innecesarios al arrancar la app con un token vencido.
const isTokenExpired = (token: string): boolean => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return typeof payload.exp === 'number' && Date.now() >= payload.exp * 1000;
  } catch {
    return true;
  }
};

const getStoredUser = (): User | null => {
  const userJson = localStorage.getItem(AUTH_CONFIG.userKey);
  if (!userJson) return null;
  try {
    const user = JSON.parse(userJson) as User;
    // Validate new format: role must be object or null, not a string
    if (user.role !== null && user.role !== undefined && typeof user.role !== 'object') {
      clearStoredAuth();
      return null;
    }
    return user;
  } catch {
    return null;
  }
};

const getStoredPermissions = (): string[] => {
  const json = localStorage.getItem(PERMISSIONS_KEY);
  if (!json) return [];
  try {
    return JSON.parse(json) as string[];
  } catch {
    return [];
  }
};

const setStoredAuth = (token: string, user: User, permissions: string[]): void => {
  localStorage.setItem(AUTH_CONFIG.tokenKey, token);
  localStorage.setItem(AUTH_CONFIG.userKey, JSON.stringify(user));
  localStorage.setItem(PERMISSIONS_KEY, JSON.stringify(permissions));
};

const clearStoredAuth = (): void => {
  localStorage.removeItem(AUTH_CONFIG.tokenKey);
  localStorage.removeItem(AUTH_CONFIG.userKey);
  localStorage.removeItem(PERMISSIONS_KEY);
};

export const useAuthStore = create<AuthStore>()(
  immer((set) => ({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: true,
    error: null,
    permissions: [],

    login: async (credentials: LoginCredentials): Promise<boolean> => {
      set((state) => {
        state.isLoading = true;
        state.error = null;
      });

      try {
        const response = await loginApi(credentials);

        if (response.success && response.user && response.token) {
          const perms = response.permissions ?? [];
          setStoredAuth(response.token, response.user, perms);
          set((state) => {
            state.user = response.user;
            state.token = response.token;
            state.isAuthenticated = true;
            state.isLoading = false;
            state.permissions = perms;
          });
          return true;
        }

        set((state) => {
          state.error = response.message ?? 'Error al iniciar sesión';
          state.isLoading = false;
        });
        return false;
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Error al iniciar sesión';
        set((state) => {
          state.error = message;
          state.isLoading = false;
        });
        return false;
      }
    },

    logout: async (): Promise<void> => {
      set((state) => {
        state.isLoading = true;
      });

      try {
        await logoutApi();
      } finally {
        clearStoredAuth();
        set((state) => {
          state.user = null;
          state.token = null;
          state.isAuthenticated = false;
          state.isLoading = false;
          state.error = null;
          state.permissions = [];
        });
      }
    },

    checkAuth: async (): Promise<void> => {
      const token = getStoredToken();
      const storedUser = getStoredUser();

      if (!token || !storedUser) {
        set((state) => {
          state.isLoading = false;
          state.isAuthenticated = false;
        });
        return;
      }

      // Verificar expiración localmente antes de hacer una llamada al API
      if (isTokenExpired(token)) {
        clearStoredAuth();
        set((state) => {
          state.isLoading = false;
          state.isAuthenticated = false;
        });
        return;
      }

      set((state) => {
        state.isLoading = true;
      });

      try {
        const response = await validateToken(token);

        if (response.success && response.user) {
          const perms = getStoredPermissions();
          set((state) => {
            state.user = response.user;
            state.token = token;
            state.isAuthenticated = true;
            state.isLoading = false;
            state.permissions = perms;
          });
        } else {
          clearStoredAuth();
          set((state) => {
            state.user = null;
            state.token = null;
            state.isAuthenticated = false;
            state.isLoading = false;
            state.permissions = [];
          });
        }
      } catch {
        clearStoredAuth();
        set((state) => {
          state.user = null;
          state.token = null;
          state.isAuthenticated = false;
          state.isLoading = false;
          state.permissions = [];
        });
      }
    },

    clearError: (): void => {
      set((state) => {
        state.error = null;
      });
    },
  }))
);

// Selectors
export const selectUser = (state: AuthStore) => state.user;
export const selectIsAuthenticated = (state: AuthStore) => state.isAuthenticated;
export const selectIsLoading = (state: AuthStore) => state.isLoading;
export const selectError = (state: AuthStore) => state.error;
export const selectPermissions = (state: AuthStore) => state.permissions;
