import { cn } from '@/shared/utils/cn';

interface StatusBadgeProps {
  active: boolean;
  activeLabel?: string;
  inactiveLabel?: string;
  className?: string;
}

export function StatusBadge({
  active,
  activeLabel = 'Activo',
  inactiveLabel = 'Inactivo',
  className,
}: StatusBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold',
        active ? 'bg-teal-50 text-teal-700' : 'bg-zinc-100 text-zinc-400',
        className
      )}
    >
      <span
        className={cn(
          'w-1.5 h-1.5 rounded-full shrink-0',
          active ? 'bg-teal-500' : 'bg-zinc-400'
        )}
      />
      {active ? activeLabel : inactiveLabel}
    </span>
  );
}
