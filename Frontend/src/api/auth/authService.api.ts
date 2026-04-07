import api from '@/api/axiosConfig';
import type { AuthResponse, LoginCredentials } from '@/features/auth/types/auth';
import type { User } from '@/features/users/types/user';
import type { Role } from '@/features/users/types/role';
import type { PermissionKey } from '@/shared/types/permission';

// Permissions are returned directly from the backend as "module.section:action" keys
// e.g. "orders.lista:view", "users.roles:manage"
function mapPermissions(raw: string[]): PermissionKey[] {
  return raw.filter((p): p is PermissionKey => /^[a-z]+\.[a-z]+:[a-z]+$/.test(p));
}

interface ApiUser {
  id: number;
  username: string;
  name: string;
  email: string;
  phone?: string | null;
  role: string | null;
  status: string;
  permissions?: string[];
  createdAt: string;
  createdBy?: number | null;
  lastLogin?: string | null;
}

interface ApiAuthResponse {
  success: boolean;
  token: string | null;
  user: ApiUser | null;
  message?: string | null;
}

function mapRole(roleName: string | null): Role | null {
  if (!roleName) return null;
  return {
    id: 0,
    name: roleName,
    description: null,
    isActive: true,
  };
}

function mapUser(apiUser: ApiUser): User {
  return {
    id: apiUser.id,
    username: apiUser.username,
    fullName: apiUser.name,
    email: apiUser.email,
    isActive: apiUser.status === 'active',
    role: mapRole(apiUser.role),
    createdAt: apiUser.createdAt,
    createdBy: apiUser.createdBy ?? undefined,
    lastLogin: apiUser.lastLogin ?? undefined,
  };
}

function mapResponse(apiResponse: ApiAuthResponse): AuthResponse {
  const user = apiResponse.user ? mapUser(apiResponse.user) : null;
  const permissions = mapPermissions(apiResponse.user?.permissions ?? []);
  return {
    success: apiResponse.success,
    token: apiResponse.token,
    user,
    permissions,
    message: apiResponse.message ?? undefined,
  };
}

export async function login(credentials: LoginCredentials): Promise<AuthResponse> {
  try {
    const response = await api.post<ApiAuthResponse>('/auth/login', credentials);
    return mapResponse(response.data);
  } catch (error) {
    console.error('Login API error:', error);
    return {
      success: false,
      user: null,
      token: null,
      permissions: [],
      message: 'Error al conectar con el servidor',
    };
  }
}

export async function validateToken(token: string): Promise<AuthResponse> {
  try {
    const response = await api.get<ApiAuthResponse>('/auth/validate', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    return mapResponse(response.data);
  } catch (error) {
    console.error('Token validation error:', error);
    return {
      success: false,
      user: null,
      token: null,
      permissions: [],
      message: 'Token inválido o expirado',
    };
  }
}

export async function logout(): Promise<void> {
  try {
    await api.post('/auth/logout');
  } catch (error) {
    console.error('Logout API error:', error);
  }
}
