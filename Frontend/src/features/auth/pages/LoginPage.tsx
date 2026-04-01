import { DeepSoftLogo } from '@/shared/components/ui/deepsoft-logo';
import { LoginForm } from '@/features/auth/components/LoginForm';

export function LoginPage() {
  return (
    <div className="min-h-screen flex bg-white">

      {/* ── Panel izquierdo (desktop) ── */}
      <div className="hidden lg:flex w-72 xl:w-80 shrink-0 bg-indigo-900 flex-col justify-between p-10 relative overflow-hidden">

        {/* Círculos decorativos sutiles */}
        <div className="absolute -bottom-20 -right-20 w-72 h-72 rounded-full border border-teal-400/15 pointer-events-none" />
        <div className="absolute -bottom-10 -right-10 w-44 h-44 rounded-full border border-teal-400/10 pointer-events-none" />

        {/* Marca */}
        <div className="relative z-10">
          <DeepSoftLogo width={48} height={48} className="mb-6" />
          <h1 className="text-2xl font-bold text-white tracking-tight leading-none">
            DeepSoft
          </h1>
          <p className="text-[10px] font-semibold text-teal-300 tracking-[0.18em] uppercase mt-2">
            Lavandería
          </p>
        </div>

        {/* Pie del panel */}
        <div className="relative z-10">
          <p className="text-sm text-indigo-300 leading-relaxed mb-8">
            Control total de tu lavandería. Órdenes, pagos y cierre de caja en un solo lugar.
          </p>
          <p className="text-[10px] text-indigo-500 tracking-wide">
            v1.0.0 · Sistema de Gestión
          </p>
        </div>
      </div>

      {/* ── Panel derecho: formulario ── */}
      <div className="flex-1 flex items-center justify-center p-8 lg:p-12 bg-blue-50/30">
        <div className="w-full max-w-sm">

          {/* Header mobile */}
          <div className="lg:hidden mb-10 text-center">
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-xl bg-indigo-900 mb-4">
              <DeepSoftLogo width={36} height={36} />
            </div>
            <h1 className="text-xl font-bold text-indigo-900 tracking-tight">DeepSoft Lavandería</h1>
            <p className="text-xs text-blue-400 mt-1">Sistema de gestión</p>
          </div>

          {/* Barra teal */}
          <div className="w-7 h-0.5 bg-teal-400 rounded-full mb-7" />

          <div className="space-y-1 mb-8">
            <h2 className="text-2xl font-bold text-indigo-900 tracking-tight">
              Iniciar sesión
            </h2>
            <p className="text-sm text-blue-400">
              Ingresa tus credenciales para acceder
            </p>
          </div>

          <LoginForm />

          <p className="text-center text-[11px] text-blue-300 mt-8">
            Acceso exclusivo para usuarios registrados
          </p>
        </div>
      </div>

    </div>
  );
}
