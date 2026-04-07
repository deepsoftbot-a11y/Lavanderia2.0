import { useEffect } from 'react';
import { AlertCircle } from 'lucide-react';
import type { DateRange } from 'react-day-picker';

import { useDashboardStore } from '../stores/dashboardStore';
import { DateRangePicker } from '@/shared/components/ui/date-range-picker';
import { DashboardCardsGrid } from '../components/DashboardCardsGrid';
import { RevenueTimelineChart } from '../components/RevenueTimelineChart';
import { OrdersByStatusChart } from '../components/OrdersByStatusChart';
import { RevenueByMethodChart } from '../components/RevenueByMethodChart';
import { RevenueByServiceChart } from '../components/RevenueByServiceChart';
import { RevenueByCategoryChart } from '../components/RevenueByCategoryChart';
import { WeeklyComparisonChart } from '../components/WeeklyComparisonChart';

export function Dashboard() {
  const { fetchDashboard, error, fechaInicio, fechaFin, setFechaRange } = useDashboardStore();

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  if (error) {
    return (
      <div className="mx-auto max-w-4xl mt-8">
        <div className="bg-rose-50 border border-rose-200 rounded-xl px-4 py-3 flex items-center gap-3">
          <AlertCircle className="h-4 w-4 text-rose-600 shrink-0" />
          <p className="text-xs font-medium text-rose-700">{error}</p>
        </div>
      </div>
    );
  }

  const dateRange: DateRange = {
    from: fechaInicio,
    to: fechaFin,
  };

  const handleDateChange = (range: DateRange) => {
    if (range?.from && range?.to) {
      setFechaRange(range.from, range.to);
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-8">
      {/* Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-1.5">
            Panorama
          </p>
          <h1 className="font-mono font-bold tabular-nums text-2xl tracking-tight leading-none text-zinc-900">
            Dashboard
          </h1>
        </div>
        <DateRangePicker date={dateRange} onDateChange={handleDateChange} />
      </div>

      {/* KPI Cards */}
      <DashboardCardsGrid />

      {/* Ingresos por día + Órdenes por estado */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <RevenueTimelineChart />
        <OrdersByStatusChart />
      </div>

      {/* Método de pago + Comparativa semanal */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <RevenueByMethodChart />
        <WeeklyComparisonChart />
      </div>

      {/* Por servicio + Por categoría */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <RevenueByServiceChart />
        <RevenueByCategoryChart />
      </div>
    </div>
  );
}
