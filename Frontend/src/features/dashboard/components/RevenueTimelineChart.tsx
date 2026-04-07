import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { format, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';
import { useDashboardStore } from '../stores/dashboardStore';
import {
  ChartPanel,
  CHART_TOOLTIP_STYLE,
  CHART_AXIS_TICK,
  CHART_GRID_STROKE,
  CHART_COLORS,
} from './ChartPanel';

const formatK = (v: number) => `$${(v / 1000).toFixed(0)}k`;

export function RevenueTimelineChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) {
    return (
      <ChartPanel label="Ingresos por Día">
        <div className="h-[260px] bg-zinc-50 rounded animate-pulse" />
      </ChartPanel>
    );
  }

  const data = charts.ingresosPorDia.map((d) => ({
    ...d,
    fechaLabel: format(parseISO(d.fecha), 'dd MMM', { locale: es }),
  }));

  const totalIngresos = data.reduce((acc, d) => acc + d.ingresos, 0);

  return (
    <ChartPanel
      label="Ingresos por Día"
      hint={`$${totalIngresos.toLocaleString('es-MX', { maximumFractionDigits: 0 })}`}
    >
      <ResponsiveContainer width="100%" height={260}>
        <LineChart data={data} margin={{ top: 10, right: 16, left: 0, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke={CHART_GRID_STROKE} vertical={false} />
          <XAxis
            dataKey="fechaLabel"
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
            cursor={{ stroke: '#e4e4e7', strokeWidth: 1 }}
            contentStyle={CHART_TOOLTIP_STYLE}
            labelStyle={{ color: '#71717a', fontSize: 10, fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.05em' }}
            formatter={(value: unknown, name: unknown) => [
              name === 'ingresos'
                ? `$${(value as number).toLocaleString('es-MX')}`
                : String(value),
              name === 'ingresos' ? 'Ingresos' : 'Órdenes',
            ]}
          />
          <Line
            type="monotone"
            dataKey="ingresos"
            stroke={CHART_COLORS.primary}
            strokeWidth={2}
            dot={false}
            activeDot={{ r: 4, strokeWidth: 0, fill: CHART_COLORS.primary }}
          />
        </LineChart>
      </ResponsiveContainer>
    </ChartPanel>
  );
}
