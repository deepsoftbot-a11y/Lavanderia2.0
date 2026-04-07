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

export function RevenueByServiceChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) {
    return (
      <ChartPanel label="Top Servicios">
        <div className="h-[260px] bg-zinc-50 rounded animate-pulse" />
      </ChartPanel>
    );
  }

  const data = charts.ingresosPorServicio.slice(0, 8);

  return (
    <ChartPanel label="Top Servicios">
      <ResponsiveContainer width="100%" height={260}>
        <BarChart
          data={data}
          layout="vertical"
          margin={{ top: 4, right: 16, left: 0, bottom: 0 }}
        >
          <CartesianGrid strokeDasharray="3 3" stroke={CHART_GRID_STROKE} horizontal={false} />
          <XAxis
            type="number"
            tick={CHART_AXIS_TICK}
            tickFormatter={formatK}
            axisLine={false}
            tickLine={false}
            tickMargin={6}
          />
          <YAxis
            dataKey="servicio"
            type="category"
            tick={{ ...CHART_AXIS_TICK, fontSize: 11, fill: '#52525b' }}
            axisLine={false}
            tickLine={false}
            width={120}
          />
          <Tooltip
            cursor={{ fill: '#fafafa' }}
            contentStyle={CHART_TOOLTIP_STYLE}
            labelStyle={{ color: '#71717a', fontSize: 10, fontWeight: 600 }}
            formatter={(v: unknown) => [`$${(v as number).toLocaleString('es-MX')}`, 'Ingresos']}
          />
          <Bar dataKey="total" fill={CHART_COLORS.primary} radius={[0, 4, 4, 0]} barSize={14} />
        </BarChart>
      </ResponsiveContainer>
    </ChartPanel>
  );
}
