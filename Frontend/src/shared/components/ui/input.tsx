import * as React from "react"

import { cn } from '@/shared/utils/cn'

const Input = React.forwardRef<HTMLInputElement, React.ComponentProps<"input">>(
  ({ className, type, ...props }, ref) => {
    return (
      <input
        type={type}
        className={cn(
          "flex h-11 w-full rounded-xl border-2 border-transparent bg-zinc-100 px-4 text-sm text-indigo-900 outline-none transition-all duration-150 placeholder:text-zinc-400 hover:bg-zinc-200 focus:border-blue-600 focus:bg-blue-50 disabled:cursor-not-allowed disabled:opacity-50 file:border-0 file:bg-transparent file:text-sm file:font-medium",
          className
        )}
        ref={ref}
        {...props}
      />
    )
  }
)
Input.displayName = "Input"

export { Input }
