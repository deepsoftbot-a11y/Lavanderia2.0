import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import { ClearableInput } from '@/shared/components/ui/field-input';
import { Textarea } from '@/shared/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';

import { permissionSchema, type PermissionFormData } from '@/features/users/schemas/permission.schema';
import type { Permission, CreatePermissionInput, UpdatePermissionInput } from '@/features/users/types/permission';

const KNOWN_MODULES = [
  'dashboard',
  'orders',
  'customers',
  'services',
  'inventory',
  'users',
  'reports',
  'settings',
];

interface PermissionFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreatePermissionInput | UpdatePermissionInput) => Promise<void>;
  permission?: Permission;
  isLoading?: boolean;
}

function FormContent({ permission, onSubmit, onClose, isLoading }: Omit<PermissionFormProps, 'open'>) {
  const isEdit = !!permission;

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<PermissionFormData>({
    resolver: zodResolver(permissionSchema),
    defaultValues: {
      name: permission?.name ?? '',
      module: permission?.module ?? '',
      description: permission?.description ?? '',
    },
  });

  const watchedModule = watch('module');

  useEffect(() => {
    if (watchedModule && !watch('name').startsWith(watchedModule + '.')) {
      setValue('name', watchedModule + '.');
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [watchedModule]);

  const handleFormSubmit = async (data: PermissionFormData) => {
    const payload: CreatePermissionInput | UpdatePermissionInput = {
      name: data.name,
      module: data.module,
      description: data.description || undefined,
    };
    await onSubmit(payload);
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)}>
      <div className="flex flex-col max-h-[82vh]">
        <div className="overflow-y-auto flex-1 px-6 py-4 space-y-4">
          <div className="space-y-1">
            <Label htmlFor="perm-module" className="text-xs text-zinc-500 font-medium">
              Módulo <span className="text-rose-500">*</span>
            </Label>
            <Select
              value={watchedModule}
              onValueChange={(val) => setValue('module', val, { shouldValidate: true })}
              disabled={isLoading}
            >
              <SelectTrigger id="perm-module">
                <SelectValue placeholder="Selecciona un módulo" />
              </SelectTrigger>
              <SelectContent>
                {KNOWN_MODULES.map((m) => (
                  <SelectItem key={m} value={m}>
                    {m}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.module && (
              <p className="text-xs text-rose-500">{errors.module.message}</p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="perm-name" className="text-xs text-zinc-500 font-medium">
              Nombre del permiso <span className="text-rose-500">*</span>
            </Label>
            <ClearableInput
              id="perm-name"
              {...register('name')}
              placeholder="modulo.accion"
              className="font-mono text-sm"
              disabled={isLoading}
              hasError={!!errors.name}
              onClear={() => setValue('name', '')}
            />
            <p className="text-[10px] text-zinc-400">Formato: modulo.accion (ej: orders.create)</p>
            {errors.name && (
              <p className="text-xs text-rose-500">{errors.name.message}</p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="perm-desc" className="text-xs text-zinc-500 font-medium">
              Descripción
            </Label>
            <Textarea
              id="perm-desc"
              {...register('description')}
              placeholder="Describe qué permite este permiso..."
              rows={3}
              disabled={isLoading}
            />
            {errors.description && (
              <p className="text-xs text-rose-500">{errors.description.message}</p>
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
            
          >
            {isEdit ? 'Guardar cambios' : 'Crear permiso'}
          </Button>
        </div>
      </div>
    </form>
  );
}

export function PermissionForm({ open, onClose, onSubmit, permission, isLoading }: PermissionFormProps) {
  return (
    <Dialog open={open} onOpenChange={(o) => { if (!o) onClose(); }}>
      <DialogContent className="max-w-sm p-0 gap-0 overflow-hidden">
        <DialogHeader className="px-6 pt-5 pb-4 border-b border-zinc-100">
          <DialogTitle className="text-sm font-semibold text-zinc-900 tracking-tight">
            {permission ? 'Editar permiso' : 'Nuevo permiso'}
          </DialogTitle>
        </DialogHeader>
        {open && (
          <FormContent
            permission={permission}
            onSubmit={onSubmit}
            onClose={onClose}
            isLoading={isLoading}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
