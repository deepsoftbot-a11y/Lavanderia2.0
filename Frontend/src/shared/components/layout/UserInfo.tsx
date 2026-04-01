import { User as UserIcon } from 'lucide-react';

import type { User } from '@/features/auth/types/auth';

interface UserInfoProps {
  user: User | null;
  compact?: boolean;
}

const ROLE_LABELS: Record<string, string> = {
  admin: 'Administrador',
  empleado: 'Empleado',
};

export function UserInfo({ user, compact = false }: UserInfoProps) {
  if (!user) return null;

  const roleName = user.role?.name ?? '';

  if (compact) {
    return (
      <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100" title={user.fullName}>
        <UserIcon className="h-4 w-4 text-blue-600" />
      </div>
    );
  }

  return (
    <div className="flex items-center gap-3">
      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-blue-100 shrink-0">
        <UserIcon className="h-4 w-4 text-blue-600" />
      </div>
      <div className="flex flex-col min-w-0">
        <span className="font-medium text-sm text-indigo-900 truncate">{user.fullName}</span>
        <span className="text-xs text-blue-500 truncate">
          {ROLE_LABELS[roleName] ?? roleName}
        </span>
      </div>
    </div>
  );
}
