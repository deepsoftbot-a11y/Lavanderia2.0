import { useDashboardStore } from '../stores/dashboardStore';
import { ChartPanel, CHART_COLORS } from './ChartPanel';

const formatMoney = (n: number) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(n);

export function RevenueByMethodChart() {
  const { kpis, isLoading } = useDashboardStore();

  if (isLoading || !kpis) {
    return (
      <ChartPanel label="Ingresos por Método de Pago">
        <div className="h-[260px] bg-zinc-50 rounded animate-pulse" />
      </ChartPanel>
    );
  }

  const total = kpis.ingresosPorMetodo.reduce((acc, m) => acc + m.total, 0);
  const sorted = [...kpis.ingresosPorMetodo].sort((a, b) => b.total - a.total);

  return (
    <ChartPanel label="Ingresos por Método de Pago" hint={formatMoney(total)}>
      <div className="px-2 py-2 space-y-3">
        {sorted.length === 0 ? (
          <p className="text-xs text-zinc-400 py-12 text-center">Sin datos</p>
        ) : (
          sorted.map((m, i) => {
            const pct = total > 0 ? (m.total / total) * 100 : 0;
            const color = CHART_COLORS.distribution[i % CHART_COLORS.distribution.length];
            return (
              <div key={m.metodo} className="space-y-1.5">
                <div className="flex items-baseline justify-between gap-3">
                  <div className="flex items-center gap-2 min-w-0">
                    <span
                      className="h-2 w-2 rounded-full shrink-0"
                      style={{ backgroundColor: color }}
                    />
                    <span className="text-xs font-medium text-zinc-700 truncate">{m.metodo}</span>
                  </div>
                  <div className="flex items-baseline gap-3 shrink-0">
                    <span className="font-mono tabular-nums text-[10px] text-zinc-400">
                      {pct.toFixed(1)}%
                    </span>
                    <span className="font-mono tabular-nums text-xs font-semibold text-zinc-900">
                      {formatMoney(m.total)}
                    </span>
                  </div>
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
