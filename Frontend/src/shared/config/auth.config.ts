export const AUTH_CONFIG = {
  tokenKey: 'auth_token',
  userKey: 'auth_user',
  redirectPaths: {
    admin: '/dashboard',
    empleado: '/orders',
    login: '/login',
  },
} as const;

export function isMockMode(): boolean {
  return import.meta.env.VITE_AUTH_MODE !== 'api';
}

export function getAuthMode(): 'mock' | 'api' {
  return import.meta.env.VITE_AUTH_MODE === 'api' ? 'api' : 'mock';
}
