import { useMemo, useState } from 'react';
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

import { usePermissionsStore } from '@/features/users/stores/permissionsStore';
import { PermissionForm } from '@/features/users/components/PermissionForm';
import type { Permission, CreatePermissionInput, UpdatePermissionInput } from '@/features/users/types/permission';
import { useToast } from '@/shared/hooks/use-toast';

function groupByModule(permissions: Permission[]): Record<string, Permission[]> {
  return permissions.reduce<Record<string, Permission[]>>((acc, p) => {
    if (!acc[p.module]) acc[p.module] = [];
    acc[p.module].push(p);
    return acc;
  }, {});
}

export function PermissionsTab() {
  const { permissions, isLoading, createPermission, updatePermission, deletePermission } =
    usePermissionsStore();
  const { toast } = useToast();

  const [formOpen, setFormOpen] = useState(false);
  const [editingPermission, setEditingPermission] = useState<Permission | undefined>();
  const [deleteTarget, setDeleteTarget] = useState<Permission | null>(null);

  const grouped = useMemo(() => groupByModule(permissions), [permissions]);
  const modules = useMemo(() => Object.keys(grouped).sort(), [grouped]);

  const handleSubmit = async (data: CreatePermissionInput | UpdatePermissionInput) => {
    if (editingPermission) {
      const result = await updatePermission(editingPermission.id, data as UpdatePermissionInput);
      if (result) {
        toast({ title: 'Permiso actualizado' });
        setFormOpen(false);
        setEditingPermission(undefined);
      } else {
        toast({ title: 'Error al actualizar permiso', variant: 'destructive' });
      }
    } else {
      const result = await createPermission(data as CreatePermissionInput);
      if (result) {
        toast({ title: 'Permiso creado' });
        setFormOpen(false);
      } else {
        toast({ title: 'Error al crear permiso', variant: 'destructive' });
      }
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    const ok = await deletePermission(deleteTarget.id);
    if (ok) {
      toast({ title: 'Permiso eliminado' });
    } else {
      toast({ title: 'Error al eliminar permiso', variant: 'destructive' });
    }
    setDeleteTarget(null);
  };

  const openEdit = (permission: Permission) => {
    setEditingPermission(permission);
    setFormOpen(true);
  };

  const openCreate = () => {
    setEditingPermission(undefined);
    setFormOpen(true);
  };

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-zinc-100">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
          {permissions.length} {permissions.length === 1 ? 'permiso' : 'permisos'}
        </p>
        <Button
          size="sm"
          onClick={openCreate}
          className="bg-zinc-900 hover:bg-zinc-800 text-white text-xs h-8"
        >
          <Plus className="h-3.5 w-3.5 mr-1.5" />
          Nuevo permiso
        </Button>
      </div>

      {/* Table header */}
      <div className="grid grid-cols-[1fr_2fr_auto] gap-4 px-6 py-2 border-b border-zinc-100 bg-zinc-50">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Permiso</p>
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Descripción</p>
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">Acciones</p>
      </div>

      {isLoading && permissions.length === 0 ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-zinc-900" />
        </div>
      ) : permissions.length === 0 ? (
        <p className="px-6 py-8 text-sm text-zinc-400 text-center">No hay permisos registrados</p>
      ) : (
        modules.map((module) => (
          <div key={module}>
            {/* Module separator */}
            <div className="px-6 py-2 bg-zinc-50 border-b border-zinc-100">
              <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                {module}
              </p>
            </div>

            {/* Permission rows */}
            {grouped[module].map((perm) => (
              <div
                key={perm.id}
                className="grid grid-cols-[1fr_2fr_auto] gap-4 items-center px-6 py-3 border-b border-zinc-100 hover:bg-zinc-50 transition-colors"
              >
                <span className="font-mono text-sm text-zinc-800">{perm.name}</span>
                <span className="text-xs text-zinc-500">{perm.description ?? '—'}</span>
                <div className="flex items-center gap-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-7 w-7 p-0"
                    onClick={() => openEdit(perm)}
                  >
                    <Pencil className="h-3.5 w-3.5 text-zinc-400" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-7 w-7 p-0 hover:text-rose-600"
                    onClick={() => setDeleteTarget(perm)}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        ))
      )}

      <PermissionForm
        open={formOpen}
        onClose={() => { setFormOpen(false); setEditingPermission(undefined); }}
        onSubmit={handleSubmit}
        permission={editingPermission}
        isLoading={isLoading}
      />

      <AlertDialog open={!!deleteTarget} onOpenChange={(o) => { if (!o) setDeleteTarget(null); }}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar permiso?</AlertDialogTitle>
            <AlertDialogDescription>
              El permiso <span className="font-mono font-semibold">{deleteTarget?.name}</span> será
              eliminado permanentemente. Los roles que lo tengan asignado perderán este permiso.
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
