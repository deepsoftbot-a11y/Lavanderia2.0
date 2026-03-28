import { useAuthStore } from '@/features/auth/stores/authStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';

export function Dashboard() {
  const user = useAuthStore((state) => state.user);

  return (
    <div className="mx-auto max-w-4xl">
      <h1 className="mb-6 text-2xl font-bold">Dashboard</h1>

      <Card>
        <CardHeader>
          <CardTitle>Bienvenido, {user?.fullName}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2 text-sm">
            <p><span className="font-medium">Usuario:</span> {user?.username}</p>
            <p><span className="font-medium">Rol:</span> {user?.role?.name === 'admin' ? 'Administrador' : 'Empleado'}</p>
            {user?.email && (
              <p><span className="font-medium">Email:</span> {user.email}</p>
            )}
          </div>
          <p className="mt-4 text-muted-foreground">
            Esta es la página de Dashboard (solo accesible para administradores).
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
