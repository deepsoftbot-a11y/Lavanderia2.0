import * as React from 'react';

import { cn } from '@/shared/utils/cn';

export interface TextareaProps
  extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {}

const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ className, ...props }, ref) => {
    return (
      <textarea
        className={cn(
          'flex min-h-[88px] w-full rounded-xl border-2 border-transparent bg-zinc-100 px-4 py-3 text-sm text-indigo-900 outline-none transition-all duration-150 placeholder:text-zinc-400 hover:bg-zinc-200 focus:border-blue-600 focus:bg-blue-50 disabled:cursor-not-allowed disabled:opacity-50',
          className
        )}
        ref={ref}
        {...props}
      />
    );
  }
);
Textarea.displayName = 'Textarea';

export { Textarea };
