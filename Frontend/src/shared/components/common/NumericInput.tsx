import { forwardRef, useEffect, useRef, useState } from 'react';
import { Input } from '@/shared/components/ui/input';
import { cn } from '@/shared/utils/cn';

interface NumericInputProps extends Omit<React.ComponentProps<'input'>, 'onChange' | 'value' | 'type'> {
  value: number;
  onChange: (value: number) => void;
  onBlur?: () => void;
  min?: number;
  max?: number;
  step?: number;
  integer?: boolean;   // true → Math.floor (para piezas)
  prefix?: string;     // ej. "$" — renderiza span posicionado absolutamente
}

export const NumericInput = forwardRef<HTMLInputElement, NumericInputProps>(
  ({ value, onChange, onBlur, min, max, step, integer = false, prefix, className, ...props }, ref) => {
    const [inputStr, setInputStr] = useState<string>(String(value));
    const isFocused = useRef(false);

    // Sincroniza inputStr desde el prop value cuando el campo no está enfocado
    // (e.g. botones +/- externos actualizan el valor)
    useEffect(() => {
      if (!isFocused.current) {
        setInputStr(String(value));
      }
    }, [value]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      setInputStr(e.target.value);
    };

    const handleFocus = () => {
      isFocused.current = true;
    };

    const handleBlur = () => {
      isFocused.current = false;

      const parsed = parseFloat(inputStr);

      if (inputStr === '' || isNaN(parsed)) {
        // Revertir al último valor válido
        setInputStr(String(value));
      } else {
        let clamped = integer ? Math.floor(parsed) : parsed;
        if (min !== undefined) clamped = Math.max(min, clamped);
        if (max !== undefined) clamped = Math.min(max, clamped);

        setInputStr(String(clamped));
        onChange(clamped);
      }

      onBlur?.();
    };

    return (
      <div className="relative">
        {prefix && (
          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-zinc-400 text-sm font-mono select-none pointer-events-none">
            {prefix}
          </span>
        )}
        <Input
          ref={ref}
          type="number"
          value={inputStr}
          step={step}
          min={min}
          max={max}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
          className={cn('font-mono tabular-nums', prefix && 'pl-7', className)}
          {...props}
        />
      </div>
    );
  }
);

NumericInput.displayName = 'NumericInput';
