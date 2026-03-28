import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { UserForm } from '@/features/users/components/UserForm';
import { useUsersStore } from '@/features/users/stores/usersStore';
import type { CreateUserInput, UpdateUserInput } from '@/features/users/types/user';

export function CreateUser() {
  const navigate = useNavigate();
  const { createUser, isLoading } = useUsersStore();

  const handleSubmit = async (data: CreateUserInput | UpdateUserInput) => {
    const user = await createUser(data as CreateUserInput);
    if (user) {
      navigate('/users');
    }
  };

  const handleCancel = () => {
    navigate('/users');
  };

  return (
    <div className="bg-white border border-zinc-200 rounded-lg overflow-hidden">
      {/* Header */}
      <div className="flex items-center gap-3 px-6 py-5 border-b border-zinc-100">
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0 shrink-0"
          onClick={() => navigate('/users')}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-xl font-semibold text-zinc-900 tracking-tight">Crear Usuario</h1>
          <p className="text-xs text-zinc-400 mt-0.5">Registra un nuevo usuario en el sistema</p>
        </div>
      </div>

      {/* Form */}
      <div className="px-6 py-5">
        <UserForm onSubmit={handleSubmit} onCancel={handleCancel} isLoading={isLoading} />
      </div>
    </div>
  );
}
