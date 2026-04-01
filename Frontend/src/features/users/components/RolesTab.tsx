import { useState } from 'react';
import { Pencil, Trash2, Plus } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { cn } from '@/shared/utils/cn';

import { useRolesStore } from '@/features/users/stores/rolesStore';
import { usePermissionsStore } from '@/features/users/stores/permissionsStore';
import { RoleForm } from '@/features/users/components/RoleForm';
import type { Role, CreateRoleInput, UpdateRoleInput } from '@/features/users/types/role';
import { useToast } from '@/shared/hooks/use-toast';

export function RolesTab() {
  const { roles, isLoading, createRole, updateRole, deleteRole } = useRolesStore();
  const { permissions } = usePermissionsStore();
  const { toast } = useToast();

  const [formOpen, setFormOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<Role | undefined>();
  const [deleteTarget, setDeleteTarget] = useState<Role | null>(null);

  const handleSubmit = async (data: CreateRoleInput | UpdateRoleInput) => {
    if (editingRole) {
      const result = await updateRole(editingRole.id, data as UpdateRoleInput);
      if (result) {
        toast({ title: 'Rol actualizado' });
        setFormOpen(false);
        setEditingRole(undefined);
      } else {
        toast({ title: 'Error al actualizar rol', variant: 'destructive' });
      }
    } else {
      const result = await createRole(data as CreateRoleInput);
      if (result) {
        toast({ title: 'Rol creado' });
        setFormOpen(false);
      } else {
        toast({ title: 'Error al crear rol', variant: 'destructive' });
      }
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    const ok = await deleteRole(deleteTarget.id);
    if (ok) {
      toast({ title: 'Rol eliminado' });
    } else {
      toast({ title: 'Error al eliminar rol', variant: 'destructive' });
    }
    setDeleteTarget(null);
  };

  const openEdit = (role: Role) => {
    setEditingRole(role);
    setFormOpen(true);
  };

  const openCreate = () => {
    setEditingRole(undefined);
    setFormOpen(true);
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-zinc-100">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
          {roles.length} {roles.length === 1 ? 'rol' : 'roles'}
        </p>
        <Button
          size="sm"
          onClick={openCreate}
          className="text-xs h-8"
        >
          <Plus className="h-3.5 w-3.5 mr-1.5" />
          Nuevo rol
        </Button>
      </div>

      {/* Table header */}
      <div className="grid grid-cols-[2fr_3fr_auto_auto_auto] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Nombre</p>
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Descripción</p>
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Estado</p>
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Permisos</p>
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Acciones</p>
      </div>

      {isLoading && roles.length === 0 ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-zinc-900" />
        </div>
      ) : roles.length === 0 ? (
        <p className="px-6 py-8 text-sm text-zinc-400 text-center">No hay roles registrados</p>
      ) : (
        roles.map((role) => (
          <div
            key={role.id}
            className="grid grid-cols-[2fr_3fr_auto_auto_auto] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
          >
            <span className="text-sm font-medium text-zinc-900">{role.name}</span>

            <span className="text-xs text-zinc-500">{role.description ?? '—'}</span>

            <span
              className={cn(
                'inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold tracking-wide leading-none',
                role.isActive
                  ? 'bg-emerald-50 text-emerald-700'
                  : 'bg-zinc-100 text-zinc-400'
              )}
            >
              {role.isActive ? 'Activo' : 'Inactivo'}
            </span>

            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs bg-zinc-100 text-zinc-600">
              {role.permissions?.length ?? '—'} permisos
            </span>

            <div className="flex items-center gap-1">
              <Button
                variant="ghost"
                size="sm"
                className="h-7 w-7 p-0"
                onClick={() => openEdit(role)}
              >
                <Pencil className="h-3.5 w-3.5 text-zinc-400" />
              </Button>
              <Button
                variant="ghost"
                size="sm"
                className="h-7 w-7 p-0 hover:text-rose-600"
                onClick={() => setDeleteTarget(role)}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </div>
          </div>
        ))
      )}

      <RoleForm
        open={formOpen}
        onClose={() => { setFormOpen(false); setEditingRole(undefined); }}
        onSubmit={handleSubmit}
        role={editingRole}
        permissions={permissions}
        isLoading={isLoading}
      />

      <AlertDialog open={!!deleteTarget} onOpenChange={(o) => { if (!o) setDeleteTarget(null); }}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar rol?</AlertDialogTitle>
            <AlertDialogDescription>
              El rol <span className="font-semibold">{deleteTarget?.name}</span> será eliminado
              permanentemente. Los usuarios que tengan este rol asignado quedarán sin rol.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-rose-600 hover:bg-rose-700 text-white"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
