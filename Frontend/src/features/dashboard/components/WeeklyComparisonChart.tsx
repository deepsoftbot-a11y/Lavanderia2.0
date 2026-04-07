import { ArrowDownRight, ArrowUpRight, Minus } from 'lucide-react';
import { useDashboardStore } from '../stores/dashboardStore';
import { ChartPanel } from './ChartPanel';
import { cn } from '@/shared/utils/cn';

const formatMoney = (n: number) =>
  new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(n);

export function WeeklyComparisonChart() {
  const { charts, isLoading } = useDashboardStore();

  if (isLoading || !charts) {
    return (
      <ChartPanel label="Comparativa Semanal">
        <div className="h-[260px] bg-zinc-50 rounded animate-pulse" />
      </ChartPanel>
    );
  }

  const { semanaActual, semanaAnterior } = charts.comparativaSemanal;
  const diff = semanaActual - semanaAnterior;
  const diffPct =
    semanaAnterior > 0 ? (diff / semanaAnterior) * 100 : semanaActual > 0 ? 100 : 0;

  const tone: 'positive' | 'negative' | 'neutral' =
    diff > 0 ? 'positive' : diff < 0 ? 'negative' : 'neutral';

  const TrendIcon = tone === 'positive' ? ArrowUpRight : tone === 'negative' ? ArrowDownRight : Minus;

  const heroToneText = {
    positive: 'text-emerald-600',
    negative: 'text-rose-600',
    neutral: 'text-zinc-300',
  }[tone];

  const badgeTone = {
    positive: 'bg-emerald-50 border-emerald-100 text-emerald-700',
    negative: 'bg-rose-50 border-rose-100 text-rose-700',
    neutral: 'bg-zinc-100 border-zinc-200 text-zinc-500',
  }[tone];

  // Proporción para la barra comparativa
  const max = Math.max(semanaActual, semanaAnterior, 1);
  const pctActual = (semanaActual / max) * 100;
  const pctAnterior = (semanaAnterior / max) * 100;

  return (
    <ChartPanel label="Comparativa Semanal">
      <div className="px-3 py-2">
        {/* Hero: diferencia */}
        <div className="flex items-end justify-between gap-3 mb-6">
          <div>
            <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400 mb-1.5">
              Diferencia
            </p>
            <p
              className={cn(
                'font-mono font-bold tabular-nums text-3xl tracking-tight leading-none',
                heroToneText,
              )}
            >
              {diff >= 0 ? '+' : ''}
              {formatMoney(diff)}
            </p>
          </div>
          <span
            className={cn(
              'inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-1 rounded-full border',
              badgeTone,
            )}
          >
            <TrendIcon className="h-3 w-3" strokeWidth={2.5} />
            {diff >= 0 ? '+' : ''}
            {diffPct.toFixed(1)}%
          </span>
        </div>

        {/* Barras comparativas */}
        <div className="space-y-4">
          <div className="space-y-1.5">
            <div className="flex items-baseline justify-between">
              <span className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                Semana Anterior
              </span>
              <span className="font-mono tabular-nums text-xs font-semibold text-zinc-500">
                {formatMoney(semanaAnterior)}
              </span>
            </div>
            <div className="h-2 bg-zinc-100 rounded-full overflow-hidden">
              <div
                className="h-full bg-zinc-300 rounded-full transition-all duration-300"
                style={{ width: `${pctAnterior}%` }}
              />
            </div>
          </div>
          <div className="space-y-1.5">
            <div className="flex items-baseline justify-between">
              <span className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
                Semana Actual
              </span>
              <span className="font-mono tabular-nums text-xs font-semibold text-zinc-900">
                {formatMoney(semanaActual)}
              </span>
            </div>
            <div className="h-2 bg-zinc-100 rounded-full overflow-hidden">
              <div
                className={cn(
                  'h-full rounded-full transition-all duration-300',
                  tone === 'positive' && 'bg-emerald-500',
                  tone === 'negative' && 'bg-rose-500',
                  tone === 'neutral' && 'bg-zinc-400',
                )}
                style={{ width: `${pctActual}%` }}
              />
            </div>
          </div>
        </div>
      </div>
    </ChartPanel>
  );
}
