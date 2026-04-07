import { CalendarIcon } from 'lucide-react';
import type { DateRange } from 'react-day-picker';

import { cn } from '@/shared/utils/cn';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';

const MESES_ES = [
  'Ene','Feb','Mar','Abr','May','Jun',
  'Jul','Ago','Sep','Oct','Nov','Dic',
];

const fmt = (d: Date) => {
  const dia = String(d.getDate()).padStart(2, '0');
  const mes = MESES_ES[d.getMonth()];
  return `${dia} ${mes}`;
};

interface DateRangePickerProps {
  date: DateRange;
  onDateChange: (date: DateRange) => void;
  className?: string;
}

export function DateRangePicker({ date, onDateChange, className }: DateRangePickerProps) {
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          className={cn(
            'h-10 px-3 rounded-xl bg-zinc-100 border-2 border-transparent text-xs font-mono tabular-nums font-semibold text-zinc-900 hover:bg-zinc-200 hover:border-zinc-300 focus:bg-blue-50 focus:border-blue-600 focus:outline-none transition-colors justify-start gap-2',
            !date && 'text-zinc-400',
            className,
          )}
        >
          <CalendarIcon className="h-4 w-4 text-zinc-400" />
          {date?.from ? (
            date.to ? (
              <span>
                {fmt(date.from)} — {fmt(date.to)} {date.from.getFullYear() !== date.to.getFullYear() ? date.to.getFullYear() : ''}
              </span>
            ) : (
              <span>{fmt(date.from)} {date.from.getFullYear()}</span>
            )
          ) : (
            'Seleccionar rango'
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="range"
          defaultMonth={date?.from}
          selected={date}
          onSelect={onDateChange as (date: unknown) => void}
          numberOfMonths={2}
        />
      </PopoverContent>
    </Popover>
  );
}
