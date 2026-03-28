import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Textarea } from '@/shared/components/ui/textarea';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { cn } from '@/shared/utils/cn';

import { roleSchema, type RoleFormData } from '@/features/users/schemas/role.schema';
import type { Role, CreateRoleInput, UpdateRoleInput } from '@/features/users/types/role';
import type { Permission } from '@/features/users/types/permission';

interface RoleFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateRoleInput | UpdateRoleInput) => Promise<void>;
  role?: Role;
  permissions: Permission[];
  isLoading?: boolean;
}

// Group permissions by module
function groupByModule(permissions: Permission[]): Record<string, Permission[]> {
  return permissions.reduce<Record<string, Permission[]>>((acc, p) => {
    if (!acc[p.module]) acc[p.module] = [];
    acc[p.module].push(p);
    return acc;
  }, {});
}

function FormContent({ role, onSubmit, onClose, permissions, isLoading }: Omit<RoleFormProps, 'open'>) {
  const isEdit = !!role;
  const grouped = groupByModule(permissions);
  const modules = Object.keys(grouped).sort();

  const existingPermissionIds = role?.permissions?.map((p) => p.id) ?? [];

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState: { errors },
  } = useForm<RoleFormData>({
    resolver: zodResolver(roleSchema),
    defaultValues: {
      name: role?.name ?? '',
      description: role?.description ?? '',
      isActive: role?.isActive ?? true,
      permissionIds: existingPermissionIds,
    },
  });

  const watchedPermissionIds = watch('permissionIds');

  const togglePermission = (id: number) => {
    const current = watchedPermissionIds ?? [];
    const updated = current.includes(id)
      ? current.filter((pid) => pid !== id)
      : [...current, id];
    setValue('permissionIds', updated, { shouldValidate: true });
  };

  const toggleModule = (modulePerms: Permission[]) => {
    const current = watchedPermissionIds ?? [];
    const moduleIds = modulePerms.map((p) => p.id);
    const allSelected = moduleIds.every((id) => current.includes(id));
    const updated = allSelected
      ? current.filter((id) => !moduleIds.includes(id))
      : [...new Set([...current, ...moduleIds])];
    setValue('permissionIds', updated, { shouldValidate: true });
  };

  const handleFormSubmit = async (data: RoleFormData) => {
    const payload: CreateRoleInput | UpdateRoleInput = {
      name: data.name,
      description: data.description || undefined,
      isActive: data.isActive,
      permissionIds: data.permissionIds,
    };
    await onSubmit(payload);
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)}>
      <div className="flex flex-col max-h-[82vh]">
        <div className="overflow-y-auto flex-1">
          {/* Datos básicos */}
          <div className="px-6 py-4 border-b border-zinc-100">
            <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-3">
              Información del rol
            </p>
            <div className="grid grid-cols-2 gap-3 mb-3">
              <div className="col-span-2 sm:col-span-1 space-y-1">
                <Label htmlFor="role-name" className="text-xs text-zinc-500 font-medium">
                  Nombre <span className="text-rose-500">*</span>
                </Label>
                <Input
                  id="role-name"
                  {...register('name')}
                  placeholder="Administrador"
                  disabled={isLoading}
                />
                {errors.name && (
                  <p className="text-xs text-rose-500">{errors.name.message}</p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="role-status" className="text-xs text-zinc-500 font-medium">
                  Estado
                </Label>
                <Controller
                  name="isActive"
                  control={control}
                  render={({ field }) => (
                    <Select
                      value={field.value ? 'true' : 'false'}
                      onValueChange={(val) => field.onChange(val === 'true')}
                      disabled={isLoading}
                    >
                      <SelectTrigger id="role-status">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="true">Activo</SelectItem>
                        <SelectItem value="false">Inactivo</SelectItem>
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
            </div>

            <div className="space-y-1">
              <Label htmlFor="role-desc" className="text-xs text-zinc-500 font-medium">
                Descripción
              </Label>
              <Textarea
                id="role-desc"
                {...register('description')}
                placeholder="Describe el alcance de este rol..."
                rows={2}
                disabled={isLoading}
              />
            </div>
          </div>

          {/* Permisos */}
          <div>
            <div className="px-6 py-2.5 bg-zinc-50 border-b border-zinc-100">
              <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                Permisos asignados
              </p>
            </div>
            {errors.permissionIds && (
              <p className="px-6 py-2 text-xs text-rose-500">{errors.permissionIds.message}</p>
            )}
            {modules.length === 0 ? (
              <p className="px-6 py-4 text-xs text-zinc-400">No hay permisos disponibles</p>
            ) : (
              modules.map((module) => {
                const modulePerms = grouped[module];
                const moduleIds = modulePerms.map((p) => p.id);
                const allSelected = moduleIds.every((id) => (watchedPermissionIds ?? []).includes(id));
                const someSelected = moduleIds.some((id) => (watchedPermissionIds ?? []).includes(id));

                return (
                  <div key={module}>
                    {/* Module header row */}
                    <button
                      type="button"
                      onClick={() => toggleModule(modulePerms)}
                      className="w-full flex items-center gap-3 px-6 py-2 bg-zinc-50 border-b border-zinc-100 hover:bg-zinc-100 transition-colors text-left"
                    >
                      <Checkbox
                        checked={allSelected}
                        className={cn(someSelected && !allSelected && 'opacity-50')}
                        onCheckedChange={() => toggleModule(modulePerms)}
                        onClick={(e) => e.stopPropagation()}
                      />
                      <span className="text-[10px] font-semibold tracking-widest uppercase text-zinc-500">
                        {module}
                      </span>
                    </button>

                    {/* Permission rows */}
                    {modulePerms.map((perm) => (
                      <div
                        key={perm.id}
                        className="flex items-center gap-3 px-6 py-2.5 border-b border-zinc-100 hover:bg-zinc-50"
                      >
                        <Checkbox
                          id={`perm-${perm.id}`}
                          checked={(watchedPermissionIds ?? []).includes(perm.id)}
                          onCheckedChange={() => togglePermission(perm.id)}
                          disabled={isLoading}
                        />
                        <label
                          htmlFor={`perm-${perm.id}`}
                          className="flex-1 flex items-center gap-3 cursor-pointer"
                        >
                          <span className="font-mono text-xs text-zinc-700">{perm.name}</span>
                          {perm.description && (
                            <span className="text-xs text-zinc-400">{perm.description}</span>
                          )}
                        </label>
                      </div>
                    ))}
                  </div>
                );
              })
            )}
          </div>
        </div>

        <div className="px-6 py-4 flex gap-3 justify-end border-t border-zinc-100 bg-zinc-50">
          <Button type="button" variant="outline" onClick={onClose} disabled={isLoading}>
            Cancelar
          </Button>
          <Button
            type="submit"
            disabled={isLoading}
            className="bg-zinc-900 hover:bg-zinc-800 text-white"
          >
            {isEdit ? 'Guardar cambios' : 'Crear rol'}
          </Button>
        </div>
      </div>
    </form>
  );
}

export function RoleForm({ open, onClose, onSubmit, role, permissions, isLoading }: RoleFormProps) {
  return (
    <Dialog open={open} onOpenChange={(o) => { if (!o) onClose(); }}>
      <DialogContent className="max-w-lg p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            {role ? 'Editar rol' : 'Nuevo rol'}
          </DialogTitle>
        </DialogHeader>
        {open && (
          <FormContent
            role={role}
            onSubmit={onSubmit}
            onClose={onClose}
            permissions={permissions}
            isLoading={isLoading}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
