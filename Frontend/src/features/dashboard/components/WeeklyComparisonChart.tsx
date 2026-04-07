import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
  LabelList,
} from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

export function WeeklyComparisonChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  const { semanaActual, semanaAnterior } = charts.comparativaSemanal;
  const diff = semanaAnterior > 0 ? ((semanaActual - semanaAnterior) / semanaAnterior) * 100 : 0;
  const diffLabel = `${diff >= 0 ? '+' : ''}${diff.toFixed(1)}%`;

  const data = [
    { label: 'Semana Anterior', value: semanaAnterior },
    { label: 'Semana Actual', value: semanaActual },
  ];

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Comparativa Semanal — {diffLabel}</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={data} margin={{ top: 20, right: 20, left: 0, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="label" tick={{ fontSize: 11 }} />
            <YAxis tick={{ fontSize: 11 }} />
            <Tooltip
              formatter={(v: unknown) => [`$${(v as number).toLocaleString('es-MX')}`, 'Ingresos']}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Bar dataKey="value" radius={[4, 4, 0, 0]}>
              <LabelList
                dataKey="value"
                formatter={(v: unknown) => `$${((v as number) / 1000).toFixed(0)}k`}
                position="top"
                style={{ fontSize: 11, fill: '#71717a' }}
              />
              <Cell fill="#f59e0b" />
              <Cell fill="#10b981" />
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
