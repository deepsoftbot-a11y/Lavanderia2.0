import { useState } from 'react';
import { Eye, EyeOff, X } from 'lucide-react';
import { cn } from '@/shared/utils/cn';

/* ── Base input con estilo filled (zinc-100, borde transparente en reposo) ── */
export function FieldInput({
  hasError,
  clearable,
  className,
  ...props
}: React.InputHTMLAttributes<HTMLInputElement> & {
  hasError?: boolean;
  clearable?: boolean;
}) {
  return (
    <input
      {...props}
      className={cn(
        'w-full h-11 rounded-xl border-2 border-transparent bg-zinc-100 px-4 outline-none transition-all duration-150 placeholder:text-zinc-400',
        clearable && 'pr-10',
        hasError
          ? 'border-rose-400 bg-rose-50 text-rose-900'
          : 'text-indigo-900 hover:bg-zinc-200 focus:border-blue-600 focus:bg-blue-50',
        props.disabled && 'opacity-50 cursor-not-allowed',
        className,
      )}
    />
  );
}

/* ── Input con botón de limpiar ── */
export function ClearableInput({
  onClear,
  hasError,
  ...props
}: React.InputHTMLAttributes<HTMLInputElement> & {
  hasError?: boolean;
  onClear: () => void;
}) {
  const hasValue = String(props.value ?? '').length > 0;

  return (
    <div className="relative">
      <FieldInput hasError={hasError} clearable {...props} />
      {hasValue && (
        <button
          type="button"
          tabIndex={-1}
          onClick={onClear}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-zinc-400 hover:text-zinc-600 transition-colors duration-150 cursor-pointer"
        >
          <X className="w-4 h-4" />
        </button>
      )}
    </div>
  );
}

/* ── Input de contraseña con toggle show/hide ── */
export function PasswordInput({
  hasError,
  ...props
}: Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> & {
  hasError?: boolean;
}) {
  const [show, setShow] = useState(false);

  return (
    <div className="relative">
      <FieldInput
        type={show ? 'text' : 'password'}
        hasError={hasError}
        clearable
        {...props}
      />
      <button
        type="button"
        tabIndex={-1}
        onClick={() => setShow((v) => !v)}
        className="absolute right-3 top-1/2 -translate-y-1/2 text-zinc-400 hover:text-zinc-600 transition-colors duration-150 cursor-pointer"
      >
        {show ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
      </button>
    </div>
  );
}
