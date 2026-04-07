import { PieChart, Pie, Cell, Legend, Tooltip, ResponsiveContainer } from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

const COLORS = ['#10b981', '#6366f1', '#f59e0b', '#ef4444', '#8b5cf6'];

export function RevenueByMethodChart() {
  const { kpis, isLoading } = useDashboardStore();

  if (isLoading || !kpis) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Ingresos por Método de Pago</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <PieChart>
            <Pie
              data={kpis.ingresosPorMetodo}
              dataKey="total"
              nameKey="metodo"
              cx="50%"
              cy="50%"
              innerRadius={60}
              outerRadius={90}
              paddingAngle={3}
            >
              {kpis.ingresosPorMetodo.map((_, i) => (
                <Cell key={i} fill={COLORS[i % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip
              formatter={(v: unknown) => [`$${(v as number).toLocaleString('es-MX')}`, 'Total']}
              contentStyle={{ fontSize: 12, borderRadius: 8 }}
            />
            <Legend wrapperStyle={{ fontSize: 12 }} />
          </PieChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
