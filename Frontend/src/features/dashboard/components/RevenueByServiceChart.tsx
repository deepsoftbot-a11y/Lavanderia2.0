import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

export function RevenueByServiceChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos por Servicio</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart
            data={charts.ingresosPorServicio.slice(0, 10)}
            margin={{ top: 5, right: 10, left: 0, bottom: 50 }}
          >
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis dataKey="servicio" tick={{ fontSize: 10 }} angle={-35} textAnchor="end" />
            <YAxis tick={{ fontSize: 11 }} />
            <Tooltip
              formatter={(v: unknown) => [`$${(v as number).toLocaleString('es-MX')}`, 'Ingresos']}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Bar dataKey="total" fill="#6366f1" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
