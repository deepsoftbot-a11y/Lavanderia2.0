import { type LucideIcon } from 'lucide-react';
import { cn } from '@/shared/utils/cn';

interface DashboardKPICardProps {
  label: string;
  value: string;
  hint?: string;
  icon: LucideIcon;
  tone?: 'neutral' | 'positive' | 'negative' | 'warning';
}

const toneClasses: Record<NonNullable<DashboardKPICardProps['tone']>, string> = {
  neutral: 'text-zinc-900',
  positive: 'text-emerald-600',
  negative: 'text-rose-600',
  warning: 'text-amber-600',
};

const hintToneClasses: Record<NonNullable<DashboardKPICardProps['tone']>, string> = {
  neutral: 'text-zinc-400',
  positive: 'text-emerald-600',
  negative: 'text-rose-600',
  warning: 'text-amber-600',
};

export function DashboardKPICard({
  label,
  value,
  hint,
  icon: Icon,
  tone = 'neutral',
}: DashboardKPICardProps) {
  return (
    <div className="bg-white border border-zinc-200 rounded-lg px-3 py-2.5 transition-colors hover:border-zinc-300">
      <div className="flex items-center justify-between gap-2">
        <p className="text-[9px] font-semibold tracking-widest uppercase text-zinc-400 truncate">
          {label}
        </p>
        <Icon className="h-3 w-3 text-zinc-300 shrink-0" strokeWidth={2.25} />
      </div>
      <p
        className={cn(
          'mt-1.5 font-mono font-bold tabular-nums text-base tracking-tight leading-none truncate',
          toneClasses[tone],
        )}
      >
        {value}
      </p>
      {hint && (
        <p
          className={cn(
            'mt-1 text-[10px] font-medium tabular-nums truncate',
            hintToneClasses[tone],
          )}
        >
          {hint}
        </p>
      )}
    </div>
  );
}
