import type { AuthResponse, LoginCredentials, User } from '@/features/auth/types/auth';
import { findMockUser, findMockUserById, generateMockToken, parseUserId } from './mockData';

const MOCK_DELAY = 500;

function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function sanitizeUser(user: User): User {
  return user;
}

export async function login(credentials: LoginCredentials): Promise<AuthResponse> {
  await delay(MOCK_DELAY);

  const user = findMockUser(credentials.username, credentials.password);

  if (!user) {
    return {
      success: false,
      user: null,
      token: null,
      permissions: [],
      message: 'Usuario o contraseña incorrectos',
    };
  }

  const token = generateMockToken(user.id);

  return {
    success: true,
    user: sanitizeUser(user),
    token,
    permissions: [],
    message: 'Inicio de sesión exitoso',
  };
}

export async function validateToken(token: string): Promise<AuthResponse> {
  await delay(MOCK_DELAY / 2);

  const userId = parseUserId(token);

  if (userId === null) {
    return {
      success: false,
      user: null,
      token: null,
      permissions: [],
      message: 'Token inválido',
    };
  }

  const user = findMockUserById(userId);

  if (!user) {
    return {
      success: false,
      user: null,
      token: null,
      permissions: [],
      message: 'Usuario no encontrado',
    };
  }

  return {
    success: true,
    user: sanitizeUser(user),
    token,
    permissions: [],
  };
}

export async function logout(): Promise<void> {
  await delay(MOCK_DELAY / 2);
}
