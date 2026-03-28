// DEPRECATED: USER_ROLES y UserRole son reemplazados por roles dinámicos desde API.
// Se mantienen para compatibilidad con shared/config/roles.config.ts
export const USER_ROLES = {
  ADMIN: 'admin',
  EMPLEADO: 'empleado',
} as const;

export type UserRole = (typeof USER_ROLES)[keyof typeof USER_ROLES];

export type { User } from '@/features/users/types/user';

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  user: import('@/features/users/types/user').User | null;
  token: string | null;
  permissions: string[];
  message?: string;
}

export interface AuthState {
  user: import('@/features/users/types/user').User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  permissions: string[];
}
