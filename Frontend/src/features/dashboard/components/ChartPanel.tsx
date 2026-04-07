interface ChartPanelProps {
  label: string;
  hint?: string;
  children: React.ReactNode;
}

export function ChartPanel({ label, hint, children }: ChartPanelProps) {
  return (
    <div className="bg-white border border-zinc-200 rounded-xl overflow-hidden">
      <div className="px-5 pt-4 pb-3 border-b border-zinc-100 flex items-baseline justify-between gap-3">
        <p className="text-[10px] font-semibold tracking-widest uppercase text-zinc-400">
          {label}
        </p>
        {hint && (
          <p className="text-[11px] font-mono tabular-nums text-zinc-500">{hint}</p>
        )}
      </div>
      <div className="px-3 py-4">{children}</div>
    </div>
  );
}

export const CHART_TOOLTIP_STYLE = {
  fontSize: 11,
  borderRadius: 12,
  border: '1px solid #e4e4e7',
  backgroundColor: 'white',
  boxShadow: '0 1px 2px rgba(0,0,0,0.04)',
  padding: '8px 12px',
  color: '#18181b',
} as const;

export const CHART_AXIS_TICK = { fontSize: 10, fill: '#a1a1aa' } as const;
export const CHART_GRID_STROKE = '#f4f4f5';

// Paleta del sistema
export const CHART_COLORS = {
  primary: '#4664FF',
  secondary: '#71717a',
  positive: '#10b981',
  negative: '#f43f5e',
  warning: '#f59e0b',
  violet: '#8b5cf6',
  // Distribución (pie/multi-categoría)
  distribution: ['#4664FF', '#10b981', '#f59e0b', '#8b5cf6', '#f43f5e', '#0ea5e9'],
} as const;
