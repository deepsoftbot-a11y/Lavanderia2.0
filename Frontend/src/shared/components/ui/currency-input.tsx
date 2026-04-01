import { forwardRef, useEffect, useRef, useState } from 'react';
import { cn } from '@/shared/utils/cn';

function formatDisplay(raw: string): string {
  if (!raw) return '';
  const [intPart, decPart] = raw.split('.');
  const formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  return decPart !== undefined ? `${formattedInt}.${decPart}` : formattedInt;
}

function toRaw(display: string): string {
  return display.replace(/,/g, '');
}

interface CurrencyInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value' | 'type'> {
  value: number | '';
  onChange: (value: number | '') => void;
  onBlur?: () => void;
  max?: number;
  hasError?: boolean;
}

export const CurrencyInput = forwardRef<HTMLInputElement, CurrencyInputProps>(
  (
    { value, onChange, onBlur, max, hasError, className, placeholder = '0.00', disabled, ...props },
    ref
  ) => {
    const toDisplay = (v: number | '') =>
      v === '' ? '' : formatDisplay(v.toFixed(2));

    const [displayStr, setDisplayStr] = useState<string>(toDisplay(value));
    const isFocused = useRef(false);

    useEffect(() => {
      if (!isFocused.current) {
        setDisplayStr(toDisplay(value));
      }
    }, [value]);

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (['e', 'E', '+', '-'].includes(e.key)) e.preventDefault();
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const raw = toRaw(e.target.value).replace(/[^\d.]/g, '');

      const parts = raw.split('.');
      if (parts.length > 2) return;
      if (parts[1] !== undefined && parts[1].length > 2) return;

      if (max !== undefined && raw !== '' && raw !== '.') {
        const num = parseFloat(raw);
        if (!isNaN(num) && num > max) return;
      }

      setDisplayStr(formatDisplay(raw));

      if (raw === '' || raw === '.') {
        if (raw === '') onChange('');
        return;
      }

      if (!raw.endsWith('.')) {
        onChange(parseFloat(raw));
      }
    };

    const handleFocus = () => {
      isFocused.current = true;
    };

    const handleBlur = () => {
      isFocused.current = false;

      const raw = toRaw(displayStr);
      if (raw === '' || raw === '.') {
        setDisplayStr('');
        onChange('');
      } else {
        const num = parseFloat(raw);
        if (!isNaN(num)) {
          setDisplayStr(formatDisplay(num.toFixed(2)));
          onChange(num);
        }
      }

      onBlur?.();
    };

    return (
      <div className="relative">
        <span
          className={cn(
            'absolute left-3 top-1/2 -translate-y-1/2 text-sm font-mono select-none pointer-events-none',
            hasError ? 'text-rose-400' : 'text-zinc-400',
            disabled && 'opacity-50'
          )}
        >
          $
        </span>
        <input
          ref={ref}
          type="text"
          inputMode="decimal"
          value={displayStr}
          placeholder={placeholder}
          disabled={disabled}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          onFocus={handleFocus}
          onBlur={handleBlur}
          className={cn(
            'w-full h-11 rounded-xl border-2 border-transparent bg-zinc-100 pl-7 pr-4 text-right font-mono tabular-nums text-sm outline-none transition-all duration-150 placeholder:text-zinc-400',
            hasError
              ? 'border-rose-400 bg-rose-50 text-rose-900'
              : 'text-indigo-900 hover:bg-zinc-200 focus:border-blue-600 focus:bg-blue-50',
            disabled && 'opacity-50 cursor-not-allowed',
            className
          )}
          {...props}
        />
      </div>
    );
  }
);

CurrencyInput.displayName = 'CurrencyInput';
