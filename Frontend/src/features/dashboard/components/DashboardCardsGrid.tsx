import {
  DollarSign,
  Percent,
  CreditCard,
  Clock,
  AlertCircle,
  Users,
  User,
  Banknote,
  Scale,
  Receipt,
} from 'lucide-react';
import { DashboardKPICard } from './DashboardKPICard';
import { useDashboardStore } from '../stores/dashboardStore';

const formatMoney = (n: number) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(n);

const formatInt = (n: number) => new Intl.NumberFormat('es-MX').format(n);

function KPISkeletonGrid() {
  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2">
      {Array.from({ length: 10 }).map((_, i) => (
        <div key={i} className="bg-white border border-zinc-200 rounded-lg px-3 py-2.5 h-[68px]">
          <div className="h-2 w-16 bg-zinc-100 rounded animate-pulse" />
          <div className="h-4 w-20 bg-zinc-100 rounded mt-2 animate-pulse" />
        </div>
      ))}
    </div>
  );
}

export function DashboardCardsGrid() {
  const { kpis, isLoading } = useDashboardStore();

  if (isLoading || !kpis) {
    return <KPISkeletonGrid />;
  }

  const ordenesPendientes = kpis.ordenesPendientesPagar;

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2">
      {/* Fila 1 — alta importancia: dinero y alertas */}
      <DashboardKPICard
        label="Ingresos Totales"
        value={formatMoney(kpis.ingresosTotales)}
        icon={DollarSign}
      />
      <DashboardKPICard
        label="Pendientes Pagar"
        value={formatMoney(ordenesPendientes.total)}
        hint={`${ordenesPendientes.cantidad} ${ordenesPendientes.cantidad === 1 ? 'orden' : 'órdenes'}`}
        icon={AlertCircle}
        tone={ordenesPendientes.cantidad > 0 ? 'negative' : 'neutral'}
      />
      <DashboardKPICard
        label="Órdenes Atrasadas"
        value={formatInt(kpis.ordenesAtrasadas)}
        icon={Clock}
        tone={kpis.ordenesAtrasadas > 0 ? 'negative' : 'neutral'}
      />
      <DashboardKPICard
        label="Ticket Promedio"
        value={formatMoney(kpis.ticketPromedio)}
        icon={CreditCard}
      />
      <DashboardKPICard
        label="Transacciones"
        value={formatInt(kpis.transacciones)}
        icon={Receipt}
      />

      {/* Fila 2 — secundarios: caja y operación */}
      <DashboardKPICard
        label="Total Corte Caja"
        value={formatMoney(kpis.totalCorteCaja)}
        icon={Banknote}
      />
      <DashboardKPICard
        label="Diferencias Caja"
        value={formatInt(kpis.diferencias)}
        icon={Scale}
        tone={kpis.diferencias > 0 ? 'negative' : 'neutral'}
      />
      <DashboardKPICard
        label="Descuentos"
        value={formatMoney(kpis.totalDescuentos)}
        icon={Percent}
      />
      <DashboardKPICard
        label="Clientes Nuevos"
        value={formatInt(kpis.clientesNuevos)}
        icon={Users}
      />
      {kpis.clienteTop ? (
        <DashboardKPICard
          label="Cliente Top"
          value={kpis.clienteTop.nombre}
          hint={`${kpis.clienteTop.ordenes} ${kpis.clienteTop.ordenes === 1 ? 'orden' : 'órdenes'}`}
          icon={User}
        />
      ) : (
        <DashboardKPICard label="Cliente Top" value="—" icon={User} />
      )}
    </div>
  );
}
