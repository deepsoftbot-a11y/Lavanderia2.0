import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormMessage,
} from '@/shared/components/ui/form';
import { ClearableInput, PasswordInput } from '@/shared/components/ui/field-input';
import { loginSchema, type LoginFormData } from '@/features/auth/schemas/auth.schema';
import { useAuthStore } from '@/features/auth/stores/authStore';

export function LoginForm() {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const login = useAuthStore((state) => state.login);

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: { username: '', password: '' },
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsSubmitting(true);
    try {
      const success = await login(data);
      if (success) {
        const permissions = useAuthStore.getState().permissions;
        const redirectPath = permissions.includes('dashboard.general:view')
          ? '/dashboard'
          : permissions.includes('orders.lista:view')
          ? '/orders'
          : permissions.includes('orders.nueva:create') || permissions.includes('orders.corte:manage')
          ? '/orders/new'
          : permissions.some((p) => p.startsWith('services.'))
          ? '/services'
          : permissions.some((p) => p.startsWith('users.'))
          ? '/users'
          : '/unauthorized';
        toast.success('Inicio de sesión exitoso');
        navigate(redirectPath, { replace: true });
      } else {
        toast.error(useAuthStore.getState().error ?? 'Error al iniciar sesión');
      }
    } catch {
      toast.error('Error al conectar con el servidor');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-3">
        {/* Usuario */}
        <FormField
          control={form.control}
          name="username"
          render={({ field, fieldState }) => (
            <FormItem className="space-y-1">
              <FormControl>
                <ClearableInput
                  placeholder="Usuario"
                  autoComplete="username"
                  disabled={isSubmitting}
                  hasError={!!fieldState.error}
                  onClear={() => form.setValue('username', '', { shouldValidate: true })}
                  {...field}
                />
              </FormControl>
              <FormMessage className="text-xs text-rose-500 px-2" />
            </FormItem>
          )}
        />

        {/* Contraseña */}
        <FormField
          control={form.control}
          name="password"
          render={({ field, fieldState }) => (
            <FormItem className="space-y-1">
              <FormControl>
                <PasswordInput
                  placeholder="Contraseña"
                  autoComplete="current-password"
                  disabled={isSubmitting}
                  hasError={!!fieldState.error}
                  {...field}
                />
              </FormControl>
              <FormMessage className="text-xs text-rose-500 px-2" />
            </FormItem>
          )}
        />

        {/* Botón */}
        <div className="pt-2">
          <Button
            type="submit"
            disabled={isSubmitting}
            className="w-full h-11 rounded-2xl bg-indigo-700 hover:bg-indigo-600 hover:shadow-lg hover:shadow-indigo-700/25 active:scale-[0.98] active:shadow-none text-white font-semibold text-base tracking-wide border-0 transition-all duration-150"
          >
            {isSubmitting ? (
              <span className="flex items-center gap-2">
                <Loader2 className="w-5 h-5 animate-spin" />
                Iniciando sesión...
              </span>
            ) : (
              'Entrar al sistema'
            )}
          </Button>
        </div>
      </form>
    </Form>
  );
}
