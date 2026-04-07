import { type LucideIcon } from 'lucide-react';
import { Card, CardContent } from '@/shared/components/ui/card';
import { TrendingDown, TrendingUp, Minus } from 'lucide-react';

interface DashboardKPICardProps {
  title: string;
  value: string;
  subtitle?: string;
  icon: LucideIcon;
  trend?: 'up' | 'down' | 'neutral';
}

export function DashboardKPICard({ title, value, subtitle, icon: Icon, trend }: DashboardKPICardProps) {
  const TrendIcon = trend === 'up' ? TrendingUp : trend === 'down' ? TrendingDown : Minus;
  const trendColor =
    trend === 'up' ? 'text-emerald-600' : trend === 'down' ? 'text-rose-600' : 'text-zinc-400';

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-4">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <p className="text-xs text-muted-foreground font-medium uppercase tracking-wide">
              {title}
            </p>
            <p className="text-2xl font-bold text-zinc-900">{value}</p>
            {subtitle && (
              <p className={`text-xs flex items-center gap-1 ${trendColor}`}>
                <TrendIcon className="h-3 w-3" />
                {subtitle}
              </p>
            )}
          </div>
          <div className="p-2 bg-primary/10 rounded-lg">
            <Icon className="h-5 w-5 text-primary" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
