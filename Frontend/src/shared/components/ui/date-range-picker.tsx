import { CalendarIcon } from 'lucide-react';
import { format } from 'date-fns';
import type { DateRange } from 'react-day-picker';

import { cn } from '@/shared/utils/cn';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';

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
            'h-10 px-3 rounded-xl bg-zinc-100 border-2 border-transparent text-xs font-mono tabular-nums font-semibold text-zinc-900 hover:bg-zinc-200 hover:border-zinc-300 focus:bg-blue-50 focus:border-blue-600 focus:outline-none transition-colors justify-start',
            !date && 'text-zinc-400',
            className,
          )}
        >
          <CalendarIcon className="mr-2 h-4 w-4 text-zinc-400" />
          {date?.from ? (
            date.to ? (
              <>
                {format(date.from, 'dd MMM')} — {format(date.to, 'dd MMM yyyy')}
              </>
            ) : (
              format(date.from, 'dd MMM yyyy')
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
