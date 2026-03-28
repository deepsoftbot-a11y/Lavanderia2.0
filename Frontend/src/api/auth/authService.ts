import { isMockMode } from '@/shared/config/auth.config';
import type { AuthResponse, LoginCredentials } from '@/features/auth/types/auth';
import * as mockService from './authService.mock';
import * as apiService from './authService.api';

export async function login(credentials: LoginCredentials): Promise<AuthResponse> {
  if (isMockMode()) {
    return mockService.login(credentials);
  }
  return apiService.login(credentials);
}

export async function validateToken(token: string): Promise<AuthResponse> {
  if (isMockMode()) {
    return mockService.validateToken(token);
  }
  return apiService.validateToken(token);
}

export async function logout(): Promise<void> {
  if (isMockMode()) {
    return mockService.logout();
  }
  return apiService.logout();
}
