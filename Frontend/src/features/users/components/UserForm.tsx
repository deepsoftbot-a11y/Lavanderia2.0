import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import { Label } from '@/shared/components/ui/label';
import { ClearableInput, PasswordInput } from '@/shared/components/ui/field-input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';

import { createUserSchema, updateUserSchema } from '@/features/users/schemas/user.schema';
import type { User, CreateUserInput, UpdateUserInput } from '@/features/users/types/user';
import { useRolesStore } from '@/features/users/stores/rolesStore';

interface UserFormProps {
  user?: User;
  onSubmit: (data: CreateUserInput | UpdateUserInput) => Promise<void>;
  isLoading?: boolean;
}

export function UserForm({ user, onSubmit, isLoading = false }: UserFormProps) {
  const isEdit = !!user;
  const { roles } = useRolesStore();
  const activeRoles = roles.filter((r) => r.isActive);

  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(isEdit ? updateUserSchema : createUserSchema),
    defaultValues: isEdit
      ? {
          fullName: user.fullName,
          email: user.email,
          roleId: user.role?.id ?? undefined,
          isActive: user.isActive,
        }
      : {
          username: '',
          fullName: '',
          email: '',
          password: '',
          confirmPassword: '',
          roleId: undefined as number | undefined,
          isActive: true,
        },
  });

  const onFormSubmit = async (data: CreateUserInput | UpdateUserInput) => {
    await onSubmit(data);
  };

  return (
    <form id="user-form" onSubmit={handleSubmit(onFormSubmit as Parameters<typeof handleSubmit>[0])} className="space-y-6">
      {/* Credentials section — create only */}
      {!isEdit && (
        <div className="space-y-3 pb-5 border-b border-zinc-100">
          <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
            Credenciales
          </p>

          <div className="space-y-1">
            <Label htmlFor="username" className="text-xs text-zinc-500 font-medium">
              Nombre de usuario <span className="text-rose-500">*</span>
            </Label>
            <ClearableInput
              id="username"
              {...register('username')}
              placeholder="usuario123"
              disabled={isLoading}
              hasError={!!('username' in errors && errors.username)}
              onClear={() => setValue('username' as never, '' as never)}
            />
            {'username' in errors && errors.username && (
              <p className="text-xs text-rose-500">{errors.username.message as string}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="password" className="text-xs text-zinc-500 font-medium">
                Contraseña <span className="text-rose-500">*</span>
              </Label>
              <PasswordInput
                id="password"
                {...register('password')}
                placeholder="••••••••"
                disabled={isLoading}
                hasError={!!errors.password}
              />
              {errors.password && (
                <p className="text-xs text-rose-500">{errors.password.message as string}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="confirmPassword" className="text-xs text-zinc-500 font-medium">
                Confirmar contraseña <span className="text-rose-500">*</span>
              </Label>
              <PasswordInput
                id="confirmPassword"
                {...register('confirmPassword')}
                placeholder="••••••••"
                disabled={isLoading}
                hasError={!!errors.confirmPassword}
              />
              {errors.confirmPassword && (
                <p className="text-xs text-rose-500">{errors.confirmPassword.message as string}</p>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Profile section */}
      <div className="space-y-3">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
          Perfil
        </p>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <Label htmlFor="fullName" className="text-xs text-zinc-500 font-medium">
              Nombre completo <span className="text-rose-500">*</span>
            </Label>
            <ClearableInput
              id="fullName"
              {...register('fullName')}
              placeholder="Juan Pérez"
              disabled={isLoading}
              hasError={!!errors.fullName}
              onClear={() => setValue('fullName', '')}
            />
            {errors.fullName && (
              <p className="text-xs text-rose-500">{errors.fullName.message as string}</p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="email" className="text-xs text-zinc-500 font-medium">
              Correo electrónico <span className="text-rose-500">*</span>
            </Label>
            <ClearableInput
              id="email"
              type="email"
              {...register('email')}
              placeholder="juan@ejemplo.com"
              disabled={isLoading}
              hasError={!!errors.email}
              onClear={() => setValue('email', '')}
            />
            {errors.email && (
              <p className="text-xs text-rose-500">{errors.email.message as string}</p>
            )}
          </div>
        </div>

        {/* Password change — edit only */}
        {isEdit && (
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="password" className="text-xs text-zinc-500 font-medium">
                Nueva contraseña{' '}
                <span className="text-zinc-400 font-normal">(opcional)</span>
              </Label>
              <PasswordInput
                id="password"
                {...register('password')}
                placeholder="••••••••"
                disabled={isLoading}
                hasError={!!errors.password}
              />
              {errors.password && (
                <p className="text-xs text-rose-500">{errors.password.message as string}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="confirmPassword" className="text-xs text-zinc-500 font-medium">
                Confirmar contraseña
              </Label>
              <PasswordInput
                id="confirmPassword"
                {...register('confirmPassword')}
                placeholder="••••••••"
                disabled={isLoading}
                hasError={!!errors.confirmPassword}
              />
              {errors.confirmPassword && (
                <p className="text-xs text-rose-500">{errors.confirmPassword.message as string}</p>
              )}
            </div>
          </div>
        )}

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <Label htmlFor="roleId" className="text-xs text-zinc-500 font-medium">
              Rol <span className="text-rose-500">*</span>
            </Label>
            <Controller
              name="roleId"
              control={control}
              render={({ field }) => (
                <Select
                  value={field.value ? String(field.value) : ''}
                  onValueChange={(val) => field.onChange(Number(val))}
                  disabled={isLoading}
                >
                  <SelectTrigger id="roleId" hasError={!!errors.roleId}>
                    <SelectValue placeholder="Selecciona un rol" />
                  </SelectTrigger>
                  <SelectContent>
                    {activeRoles.map((r) => (
                      <SelectItem key={r.id} value={String(r.id)}>
                        {r.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {errors.roleId && (
              <p className="text-xs text-rose-500">{errors.roleId.message as string}</p>
            )}
          </div>

          <div className="space-y-1">
            <Label htmlFor="isActive" className="text-xs text-zinc-500 font-medium">
              Estado
            </Label>
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => (
                <Select
                  value={field.value !== undefined ? String(field.value) : 'true'}
                  onValueChange={(val) => field.onChange(val === 'true')}
                  disabled={isLoading}
                >
                  <SelectTrigger id="isActive">
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
      </div>
    </form>
  );
}
