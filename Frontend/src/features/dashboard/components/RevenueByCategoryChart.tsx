import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import {
  ChartPanel,
  CHART_TOOLTIP_STYLE,
  CHART_AXIS_TICK,
  CHART_GRID_STROKE,
  CHART_COLORS,
} from './ChartPanel';

const formatK = (v: number) => `$${(v / 1000).toFixed(0)}k`;

export function RevenueByCategoryChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) {
    return (
      <ChartPanel label="Ingresos por Categoría">
        <div className="h-[260px] bg-zinc-50 rounded animate-pulse" />
      </ChartPanel>
    );
  }

  return (
    <ChartPanel label="Ingresos por Categoría">
      <ResponsiveContainer width="100%" height={260}>
        <BarChart
          data={charts.ingresosPorCategoria}
          margin={{ top: 10, right: 16, left: 0, bottom: 0 }}
        >
          <CartesianGrid strokeDasharray="3 3" stroke={CHART_GRID_STROKE} vertical={false} />
          <XAxis
            dataKey="categoria"
            tick={CHART_AXIS_TICK}
            axisLine={false}
            tickLine={false}
            tickMargin={8}
          />
          <YAxis
            tick={CHART_AXIS_TICK}
            tickFormatter={formatK}
            axisLine={false}
            tickLine={false}
            tickMargin={8}
            width={48}
          />
          <Tooltip
            cursor={{ fill: '#fafafa' }}
            contentStyle={CHART_TOOLTIP_STYLE}
            labelStyle={{ color: '#71717a', fontSize: 10, fontWeight: 600 }}
            formatter={(v: unknown) => [`$${(v as number).toLocaleString('es-MX')}`, 'Ingresos']}
          />
          <Bar dataKey="total" fill={CHART_COLORS.primary} radius={[4, 4, 0, 0]} maxBarSize={48} />
        </BarChart>
      </ResponsiveContainer>
    </ChartPanel>
  );
}
