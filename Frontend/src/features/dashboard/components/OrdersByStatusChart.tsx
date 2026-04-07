import { useDashboardStore } from '../stores/dashboardStore';
import { ChartPanel } from './ChartPanel';

const STATUS_COLORS: Record<string, string> = {
  Recibida: '#4664FF',
  'En Proceso': '#f59e0b',
  'Lista para Entregar': '#8b5cf6',
  Entregada: '#10b981',
  Cancelada: '#f43f5e',
};

export function OrdersByStatusChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) {
    return (
      <ChartPanel label="Órdenes por Estado">
        <div className="h-[260px] bg-zinc-50 rounded animate-pulse" />
      </ChartPanel>
    );
  }

  const total = charts.ordenesPorEstado.reduce((acc, e) => acc + e.cantidad, 0);
  const max = Math.max(...charts.ordenesPorEstado.map((e) => e.cantidad), 1);

  return (
    <ChartPanel label="Órdenes por Estado" hint={`${total} en total`}>
      <div className="px-2 py-2 space-y-3">
        {charts.ordenesPorEstado.length === 0 ? (
          <p className="text-xs text-zinc-400 py-12 text-center">Sin datos</p>
        ) : (
          charts.ordenesPorEstado.map((entry) => {
            const pct = (entry.cantidad / max) * 100;
            const color = STATUS_COLORS[entry.estado] ?? '#71717a';
            return (
              <div key={entry.estado} className="space-y-1.5">
                <div className="flex items-baseline justify-between gap-3">
                  <span className="text-xs font-medium text-zinc-700">{entry.estado}</span>
                  <span className="font-mono tabular-nums text-xs font-semibold text-zinc-900">
                    {entry.cantidad}
                  </span>
                </div>
                <div className="h-1.5 bg-zinc-100 rounded-full overflow-hidden">
                  <div
                    className="h-full rounded-full transition-all duration-300"
                    style={{ width: `${pct}%`, backgroundColor: color }}
                  />
                </div>
              </div>
            );
          })
        )}
      </div>
    </ChartPanel>
  );
}
