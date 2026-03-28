import { User as UserIcon } from 'lucide-react';

import type { User } from '@/features/auth/types/auth';

interface UserInfoProps {
  user: User | null;
}

const ROLE_LABELS: Record<string, string> = {
  admin: 'Administrador',
  empleado: 'Empleado',
};

export function UserInfo({ user }: UserInfoProps) {
  if (!user) return null;

  const roleName = user.role?.name ?? '';

  return (
    <div className="flex items-center gap-3 px-2">
      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-muted">
        <UserIcon className="h-5 w-5 text-muted-foreground" />
      </div>
      <div className="flex flex-col">
        <span className="font-medium text-sm">{user.fullName}</span>
        <span className="text-xs text-muted-foreground">
          {ROLE_LABELS[roleName] ?? roleName}
        </span>
      </div>
    </div>
  );
}
