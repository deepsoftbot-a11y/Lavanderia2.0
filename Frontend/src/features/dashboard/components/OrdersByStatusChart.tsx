import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';
import { useDashboardStore } from '../stores/dashboardStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';

const STATUS_COLORS: Record<string, string> = {
  Recibida: '#3b82f6',
  'En Proceso': '#f59e0b',
  'Lista para Entregar': '#8b5cf6',
  Entregada: '#10b981',
  Cancelada: '#ef4444',
};

export function OrdersByStatusChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) return <Skeleton className="h-72 w-full" />;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-medium">Órdenes por Estado</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart
            data={charts.ordenesPorEstado}
            layout="vertical"
            margin={{ top: 5, right: 30, left: 0, bottom: 5 }}
          >
            <CartesianGrid strokeDasharray="3 3" className="stroke-zinc-200" />
            <XAxis type="number" tick={{ fontSize: 11 }} />
            <YAxis dataKey="estado" type="category" tick={{ fontSize: 11 }} width={120} />
            <Tooltip contentStyle={{ fontSize: 12, borderRadius: 8 }} />
            <Bar dataKey="cantidad" radius={[0, 4, 4, 0]}>
              {charts.ordenesPorEstado.map((entry) => (
                <Cell key={entry.estado} fill={STATUS_COLORS[entry.estado] ?? '#6366f1'} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
