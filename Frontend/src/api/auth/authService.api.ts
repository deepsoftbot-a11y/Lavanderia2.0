import api from '@/api/axiosConfig';
import type { AuthResponse, LoginCredentials } from '@/features/auth/types/auth';
import type { User } from '@/features/users/types/user';
import type { Role } from '@/features/users/types/role';
import type { PermissionKey } from '@/shared/types/permission';

// Maps DB NombrePermiso (Spanish) → frontend PermissionKey (module:action)
const PERMISSION_MAP: Record<string, PermissionKey> = {
  Ver_Dashboard:       'dashboard:view',
  Crear_Usuario:       'users:create',
  Modificar_Usuario:   'users:edit',
  Eliminar_Usuario:    'users:delete',
  Ver_Usuarios:        'users:view',
  Asignar_Roles:       'users:manage',
  Crear_Cliente:       'customers:create',
  Modificar_Cliente:   'customers:edit',
  Ver_Clientes:        'customers:view',
  Gestionar_Credito:   'customers:manage',
  Ver_Saldo_Cliente:   'customers:view',
  Crear_Orden:         'orders:create',
  Modificar_Orden:     'orders:edit',
  Cancelar_Orden:      'orders:delete',
  Ver_Ordenes:         'orders:view',
  Cambiar_Estado_Orden:'orders:manage',
  Entregar_Orden:      'orders:manage',
  Crear_Servicio:      'services:manage',
  Modificar_Servicio:  'services:manage',
  Modificar_Precios:   'services:manage',
  Ver_Servicios:       'services:view',
  Aplicar_Descuento:   'services:manage',
  Crear_Combo:         'services:manage',
  Modificar_Combo:     'services:manage',
  Generar_Reporte:     'reports:view',
  Configurar_Reporte:  'reports:export',
  Configurar_Sistema:  'settings:manage',
  Ver_Auditoria:       'settings:view',
  Gestionar_Ubicaciones:'settings:manage',
};

function mapPermissions(raw: string[]): PermissionKey[] {
  const seen = new Set<PermissionKey>();
  for (const p of raw) {
    const key = PERMISSION_MAP[p];
    if (key) seen.add(key);
  }
  return [...seen];
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
