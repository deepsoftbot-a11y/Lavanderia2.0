import type { User } from '@/features/users/types/user';

interface MockUserWithPassword extends User {
  password: string;
}

const MOCK_USERS: MockUserWithPassword[] = [
  {
    id: 1,
    username: 'admin',
    fullName: 'Administrador',
    email: 'admin@lavanderia.com',
    isActive: true,
    role: { id: 1, name: 'admin', description: 'Administrador del sistema', isActive: true },
    createdAt: new Date().toISOString(),
    password: 'admin123',
  },
  {
    id: 2,
    username: 'empleado',
    fullName: 'Empleado General',
    email: 'empleado@lavanderia.com',
    isActive: true,
    role: { id: 2, name: 'empleado', description: 'Empleado del sistema', isActive: true },
    createdAt: new Date().toISOString(),
    password: 'empleado123',
  },
];

export function findMockUser(username: string, password: string): User | undefined {
  const user = MOCK_USERS.find((u) => u.username === username && u.password === password);
  if (!user) return undefined;

  const { password: _, ...userWithoutPassword } = user;
  return userWithoutPassword;
}

export function findMockUserById(id: number): User | undefined {
  const user = MOCK_USERS.find((u) => u.id === id);
  if (!user) return undefined;

  const { password: _, ...userWithoutPassword } = user;
  return userWithoutPassword;
}

export function generateMockToken(userId: number): string {
  return `mock_token_${userId}_${Date.now()}`;
}

export function parseUserId(token: string): number | null {
  const match = token.match(/^mock_token_(\d+)_/);
  return match ? Number(match[1]) : null;
}
