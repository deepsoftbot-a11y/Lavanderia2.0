import { useEffect } from 'react';
import { useDashboardStore } from '../stores/dashboardStore';
import { DashboardCardsGrid } from '../components/DashboardCardsGrid';
import { RevenueTimelineChart } from '../components/RevenueTimelineChart';
import { OrdersByStatusChart } from '../components/OrdersByStatusChart';
import { RevenueByMethodChart } from '../components/RevenueByMethodChart';
import { RevenueByServiceChart } from '../components/RevenueByServiceChart';
import { RevenueByCategoryChart } from '../components/RevenueByCategoryChart';
import { WeeklyComparisonChart } from '../components/WeeklyComparisonChart';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Separator } from '@/shared/components/ui/separator';
import { AlertCircle } from 'lucide-react';

export function Dashboard() {
  const { fetchDashboard, error, fechaInicio, fechaFin, setFechaRange } = useDashboardStore();

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  if (error) {
    return (
      <div className="mx-auto max-w-4xl mt-8">
        <Card className="border-rose-200 bg-rose-50">
          <CardContent className="flex items-center gap-3 p-4">
            <AlertCircle className="h-5 w-5 text-rose-600" />
            <p className="text-sm text-rose-700">{error}</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header con filtro de fechas */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-zinc-900">Dashboard</h1>
        <div className="flex items-center gap-2">
          <input
            type="date"
            value={fechaInicio.toISOString().split('T')[0]}
            onChange={(e) => setFechaRange(new Date(e.target.value), fechaFin)}
            className="border rounded px-3 py-1.5 text-sm"
          />
          <span className="text-zinc-400">—</span>
          <input
            type="date"
            value={fechaFin.toISOString().split('T')[0]}
            onChange={(e) => setFechaRange(fechaInicio, new Date(e.target.value))}
            className="border rounded px-3 py-1.5 text-sm"
          />
        </div>
      </div>

      {/* KPI Cards */}
      <DashboardCardsGrid />

      <Separator />

      {/* Ingresos por día + Órdenes por estado */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RevenueTimelineChart />
        <OrdersByStatusChart />
      </div>

      {/* Método de pago + Comparativa semanal */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RevenueByMethodChart />
        <WeeklyComparisonChart />
      </div>

      {/* Por servicio + Por categoría */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RevenueByServiceChart />
        <RevenueByCategoryChart />
      </div>
    </div>
  );
}
