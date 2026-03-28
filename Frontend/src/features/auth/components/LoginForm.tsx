import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import { loginSchema, type LoginFormData } from '@/features/auth/schemas/auth.schema';
import { useAuthStore } from '@/features/auth/stores/authStore';

export function LoginForm() {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const login = useAuthStore((state) => state.login);

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      username: '',
      password: '',
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsSubmitting(true);

    try {
      const success = await login(data);

      if (success) {
        const permissions = useAuthStore.getState().permissions;
        const redirectPath = permissions.includes('dashboard:view')
          ? '/dashboard'
          : permissions.includes('orders:view')
          ? '/orders'
          : '/orders/new';

        toast.success('Inicio de sesión exitoso');
        navigate(redirectPath, { replace: true });
      } else {
        const error = useAuthStore.getState().error;
        toast.error(error ?? 'Error al iniciar sesión');
      }
    } catch {
      toast.error('Error al conectar con el servidor');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5">
        <FormField
          control={form.control}
          name="username"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-sm font-medium text-foreground">
                Usuario
              </FormLabel>
              <FormControl>
                <Input
                  placeholder="Ingrese su usuario"
                  autoComplete="username"
                  disabled={isSubmitting}
                  className="h-11 transition-all duration-200 focus-visible:ring-2 focus-visible:ring-primary/20 focus-visible:border-primary"
                  {...field}
                />
              </FormControl>
              <FormMessage className="text-xs" />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-sm font-medium text-foreground">
                Contraseña
              </FormLabel>
              <FormControl>
                <Input
                  type="password"
                  placeholder="Ingrese su contraseña"
                  autoComplete="current-password"
                  disabled={isSubmitting}
                  className="h-11 transition-all duration-200 focus-visible:ring-2 focus-visible:ring-primary/20 focus-visible:border-primary"
                  {...field}
                />
              </FormControl>
              <FormMessage className="text-xs" />
            </FormItem>
          )}
        />

        <Button
          type="submit"
          className="w-full h-11 text-[15px] font-medium shadow-sm transition-all duration-200 hover:shadow-md active:scale-[0.98]"
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <span className="flex items-center gap-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              <span>Iniciando sesión...</span>
            </span>
          ) : (
            'Iniciar Sesión'
          )}
        </Button>
      </form>
    </Form>
  );
}
