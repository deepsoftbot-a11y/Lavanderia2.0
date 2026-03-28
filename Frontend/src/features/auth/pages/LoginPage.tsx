import { LoginForm } from '@/features/auth/components/LoginForm';

export function LoginPage() {
  return (
    <div className="min-h-screen grid lg:grid-cols-[1fr_1.2fr] bg-background">
      {/* Left Panel - Brand & Welcome */}
      <div className="relative hidden lg:flex flex-col justify-between p-12 bg-gradient-to-br from-primary via-primary to-[hsl(205_85%_42%)] overflow-hidden">
        {/* Subtle texture overlay */}
        <div className="absolute inset-0 opacity-[0.03]"
             style={{
               backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='1'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v6h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
             }}
        />

        {/* Circular accent - evokes washing machine drum */}
        <div className="absolute top-1/2 -right-32 w-96 h-96 rounded-full border border-white/10" />
        <div className="absolute top-1/2 -right-24 w-80 h-80 rounded-full border border-white/10" />

        <div className="relative z-10">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-white/10 backdrop-blur-sm border border-white/20 mb-6">
            <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
            </svg>
          </div>
          <h1 className="text-4xl font-semibold text-white mb-3 tracking-tight">
            DeepSoft
          </h1>
          <p className="text-xl text-white/90 font-light">
            Lavandería
          </p>
        </div>

        <div className="relative z-10 space-y-6">
          <blockquote className="space-y-2">
            <p className="text-lg text-white/95 leading-relaxed">
              "La solución completa para gestionar tu lavandería con precisión y confianza profesional."
            </p>
          </blockquote>

          <div className="flex items-center gap-4 pt-4 border-t border-white/10">
            <div className="flex -space-x-3">
              <div className="w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm border-2 border-white/30 flex items-center justify-center text-white text-sm font-medium">
                ✓
              </div>
              <div className="w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm border-2 border-white/30 flex items-center justify-center text-white text-sm font-medium">
                ✓
              </div>
              <div className="w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm border-2 border-white/30 flex items-center justify-center text-white text-sm font-medium">
                ✓
              </div>
            </div>
            <div>
              <p className="text-white/90 text-sm font-medium">Control total</p>
              <p className="text-white/70 text-xs">Pedidos, inventario y pagos</p>
            </div>
          </div>
        </div>

        <div className="relative z-10 text-white/60 text-xs">
          v1.0.0 • Sistema de Gestión
        </div>
      </div>

      {/* Right Panel - Login Form */}
      <div className="flex items-center justify-center p-8 lg:p-12">
        <div className="w-full max-w-md">
          {/* Mobile header */}
          <div className="lg:hidden mb-8 text-center">
            <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-primary/10 mb-4">
              <svg className="w-7 h-7 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
              </svg>
            </div>
            <h1 className="text-2xl font-semibold text-foreground mb-1">
              DeepSoft Lavandería
            </h1>
            <p className="text-muted-foreground text-sm">
              Gestión profesional de lavandería
            </p>
          </div>

          <div className="space-y-6">
            <div className="space-y-2">
              <h2 className="text-2xl font-semibold tracking-tight text-foreground">
                Iniciar Sesión
              </h2>
              <p className="text-muted-foreground text-sm">
                Ingrese sus credenciales para acceder al sistema
              </p>
            </div>

            <LoginForm />

            <div className="pt-4 border-t border-border">
              <p className="text-center text-xs text-muted-foreground">
                Acceso exclusivo para usuarios registrados
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
