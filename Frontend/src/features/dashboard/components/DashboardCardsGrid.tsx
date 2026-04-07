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
import { Skeleton } from '@/shared/components/ui/skeleton';

const formatMoney = (n: number) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(n);

export function DashboardCardsGrid() {
  const { kpis, isLoading } = useDashboardStore();

  if (isLoading || !kpis) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {Array.from({ length: 9 }).map((_, i) => (
          <Skeleton key={i} className="h-28" />
        ))}
      </div>
    );
  }

  const ordenesPendientes = kpis.ordenesPendientesPagar;

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      {/* Financieras */}
      <DashboardKPICard title="Ingresos Totales" value={formatMoney(kpis.ingresosTotales)} icon={DollarSign} />
      <DashboardKPICard title="Ticket Promedio" value={formatMoney(kpis.ticketPromedio)} icon={CreditCard} />
      <DashboardKPICard title="Descuentos" value={formatMoney(kpis.totalDescuentos)} icon={Percent} />
      {/* Operacionales */}
      <DashboardKPICard
        title="Órdenes Atrasadas"
        value={String(kpis.ordenesAtrasadas)}
        icon={Clock}
        trend={kpis.ordenesAtrasadas > 0 ? 'down' : 'up'}
      />
      <DashboardKPICard
        title="Pendientes por Pagar"
        value={`${ordenesPendientes.cantidad} órdenes`}
        subtitle={formatMoney(ordenesPendientes.total)}
        icon={AlertCircle}
        trend={ordenesPendientes.cantidad > 0 ? 'down' : 'neutral'}
      />
      {/* Clientes */}
      <DashboardKPICard title="Clientes Nuevos" value={String(kpis.clientesNuevos)} icon={Users} />
      {kpis.clienteTop && (
        <DashboardKPICard
          title="Cliente Top"
          value={kpis.clienteTop.nombre}
          subtitle={`${kpis.clienteTop.ordenes} órdenes`}
          icon={User}
        />
      )}
      {/* Caja */}
      <DashboardKPICard title="Total Corte Caja" value={formatMoney(kpis.totalCorteCaja)} icon={Banknote} />
      <DashboardKPICard
        title="Diferencias"
        value={String(kpis.diferencias)}
        icon={Scale}
        trend={kpis.diferencias > 0 ? 'down' : 'up'}
      />
      <DashboardKPICard title="Transacciones" value={String(kpis.transacciones)} icon={Receipt} />
    </div>
  );
}
