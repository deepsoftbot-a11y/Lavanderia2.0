import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import { format, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

const formatMoney = (v: number) => `${(v / 1000).toFixed(0)}k`;

export function RevenueTimelineChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  const data = charts.ingresosPorDia.map((d) => ({
    ...d,
    fecha: format(parseISO(d.fecha), 'dd MMM', { locale: es }),
  }));

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos y Órdenes por Día</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <LineChart data={data} margin={{ top: 5, right: 10, left: 0, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="fecha" tick={{ fontSize: 11 }} />
            <YAxis yAxisId="left" tickFormatter={formatMoney} tick={{ fontSize: 11 }} />
            <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 11 }} />
            <Tooltip
              formatter={(value: unknown, name: unknown) => [
                name === 'ingresos'
                  ? `$${(value as number).toLocaleString('es-MX')}`
                  : String(value),
                name === 'ingresos' ? 'Ingresos' : 'Órdenes',
              ]}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
            <Line
              yAxisId="left"
              type="monotone"
              dataKey="ingresos"
              stroke="#6366f1"
              strokeWidth={2}
              dot={false}
              name="Ingresos"
            />
            <Line
              yAxisId="right"
              type="monotone"
              dataKey="ordenes"
              stroke="#f59e0b"
              strokeWidth={2}
              dot={false}
              name="Órdenes"
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
