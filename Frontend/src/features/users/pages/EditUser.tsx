import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { UserForm } from '@/features/users/components/UserForm';
import { useUsersStore } from '@/features/users/stores/usersStore';
import type { UpdateUserInput } from '@/features/users/types/user';

export function EditUser() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { selectedUser, isLoading, error, fetchUserById, updateUser } = useUsersStore();

  useEffect(() => {
    if (id) {
      fetchUserById(Number(id));
    }
  }, [id, fetchUserById]);

  const handleSubmit = async (data: UpdateUserInput) => {
    if (!id) return;
    const user = await updateUser(Number(id), data);
    if (user) {
      navigate('/users');
    }
  };

  const handleCancel = () => {
    navigate('/users');
  };

  if (error) {
    return (
      <div className="bg-white border border-zinc-200 rounded-lg overflow-hidden">
        <div className="px-6 py-12 text-center">
          <p className="text-sm text-rose-500 mb-3">Error: {error}</p>
          <Button size="sm" variant="outline" onClick={() => navigate('/users')}>
            Volver a la lista
          </Button>
        </div>
      </div>
    );
  }

  if (isLoading || !selectedUser) {
    return (
      <div className="flex justify-center py-12">
        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-zinc-900" />
      </div>
    );
  }

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
          <h1 className="text-xl font-semibold text-zinc-900 tracking-tight">Editar Usuario</h1>
          <p className="text-xs text-zinc-400 mt-0.5">
            Actualiza la información de {selectedUser.fullName}
          </p>
        </div>
      </div>

      {/* Form */}
      <div className="px-6 py-5">
        <UserForm
          user={selectedUser}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
          isLoading={isLoading}
        />
      </div>
    </div>
  );
}
