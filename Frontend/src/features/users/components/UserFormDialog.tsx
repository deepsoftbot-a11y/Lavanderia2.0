import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { UserForm } from '@/features/users/components/UserForm';
import { useUsersStore } from '@/features/users/stores/usersStore';
import { useToast } from '@/shared/hooks/use-toast';
import type { User, CreateUserInput, UpdateUserInput } from '@/features/users/types/user';

interface UserFormDialogProps {
  open: boolean;
  onClose: () => void;
  user?: User | null;
}

export function UserFormDialog({ open, onClose, user }: UserFormDialogProps) {
  const { createUser, updateUser, isLoading } = useUsersStore();
  const { toast } = useToast();
  const [submitError, setSubmitError] = useState<string | null>(null);

  const handleSubmit = async (data: CreateUserInput | UpdateUserInput) => {
    setSubmitError(null);
    if (user) {
      const updated = await updateUser(user.id, data as UpdateUserInput);
      if (updated) {
        toast({ title: 'Usuario actualizado correctamente' });
        onClose();
      } else {
        setSubmitError(useUsersStore.getState().error ?? 'Error al actualizar');
      }
    } else {
      const created = await createUser(data as CreateUserInput);
      if (created) {
        toast({ title: 'Usuario creado correctamente' });
        onClose();
      } else {
        setSubmitError(useUsersStore.getState().error ?? 'Error al crear');
      }
    }
  };

  return (
    <Dialog open={open} onOpenChange={(o) => { if (!o) onClose(); }}>
      <DialogContent className="max-w-lg p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100 shrink-0">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            {user ? 'Editar usuario' : 'Nuevo usuario'}
          </DialogTitle>
        </DialogHeader>

        <div className="flex flex-col max-h-[82vh]">
          <div className="overflow-y-auto flex-1">
            {open && (
              <div className="px-6 py-5">
                <UserForm
                  user={user ?? undefined}
                  onSubmit={handleSubmit}
                  isLoading={isLoading}
                />
                {submitError && (
                  <p className="text-xs text-rose-500 mt-3">{submitError}</p>
                )}
              </div>
            )}
          </div>

          <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50 shrink-0">
            <Button variant="outline" onClick={onClose} disabled={isLoading}>
              Cancelar
            </Button>
            <Button
              form="user-form"
              type="submit"
              disabled={isLoading}
            >
              {user ? 'Actualizar' : 'Crear usuario'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
