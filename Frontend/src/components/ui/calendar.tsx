import * as React from "react"
import {
  ChevronLeftIcon,
  ChevronRightIcon,
} from "lucide-react"
import { DayButton, DayPicker, getDefaultClassNames } from "react-day-picker"
import type { Matcher } from "react-day-picker"

import { cn } from "@/shared/utils/cn"
import { Button, buttonVariants } from "@/shared/components/ui/button"

// ─── Spanish formatters ────────────────────────────────────────────
const MESES_ES = [
  "Enero","Febrero","Marzo","Abril","Mayo","Junio",
  "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre",
]

const MESES_CORTOS_ES = [
  "Ene","Feb","Mar","Abr","May","Jun",
  "Jul","Ago","Sep","Oct","Nov","Dic",
]

const DIAS_ES = ["Dom","Lun","Mar","Mié","Jue","Vie","Sáb"]

// ─── Custom Day Button ────────────────────────────────────────────
function CalendarDayButton({
  className,
  day,
  modifiers,
  ...props
}: React.ComponentProps<typeof DayButton>) {
  const defaultClassNames = getDefaultClassNames()

  const ref = React.useRef<HTMLButtonElement>(null)
  React.useEffect(() => {
    if (modifiers.focused) ref.current?.focus()
  }, [modifiers.focused])

  return (
    <Button
      ref={ref}
      variant="ghost"
      size="icon"
      data-day={day.date.toLocaleDateString()}
      data-selected-single={
        modifiers.selected &&
        !modifiers.range_start &&
        !modifiers.range_end &&
        !modifiers.range_middle
      }
      data-range-start={modifiers.range_start}
      data-range-end={modifiers.range_end}
      data-range-middle={modifiers.range_middle}
      className={cn(
        "flex aspect-square h-auto w-full min-w-[--cell-size] flex-col gap-0.5 p-0 font-normal leading-none " +
        "[&>span]:text-[11px] [&>span]:opacity-70 " +
        "hover:bg-zinc-100 " +
        "data-[selected-single=true]:bg-zinc-100 data-[selected-single=true]:text-zinc-800 " +
        "data-[range-start=true]:bg-indigo-600 data-[range-start=true]:text-white data-[range-end=true]:bg-indigo-600 data-[range-end=true]:text-white " +
        "data-[range-middle=true]:bg-zinc-50 data-[range-middle=true]:text-zinc-700 " +
        "data-[today=true]:bg-indigo-50 data-[today=true]:text-indigo-700 " +
        "data-[outside=true]:text-zinc-300 data-[outside=true]:opacity-40 " +
        "group-data-[focused=true]/day:ring-2 group-data-[focused=true]/day:ring-indigo-300 group-data-[focused=true]/day:z-10",
        defaultClassNames.day,
        className
      )}
      {...props}
    />
  )
}

// ─── Disabled matcher: future dates ────────────────────────────────
const tomorrow = new Date()
tomorrow.setHours(0,0,0,0)
tomorrow.setDate(tomorrow.getDate() + 1)
const disabled_future: Matcher = { after: tomorrow }

// ─── Main Calendar ────────────────────────────────────────────────
function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  captionLayout = "label",
  buttonVariant = "ghost",
  formatters,
  components,
  ...props
}: React.ComponentProps<typeof DayPicker> & {
  buttonVariant?: React.ComponentProps<typeof Button>["variant"]
}) {
  const defaultClassNames = getDefaultClassNames()

  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      disabled={disabled_future}
      className={cn(
        "group/calendar p-3 [--cell-size:2rem] text-zinc-700",
        String.raw`rtl:**:[.rdp-button\_next>svg]:rotate-180`,
        String.raw`rtl:**:[.rdp-button\_previous>svg]:rotate-180`,
        className
      )}
      captionLayout={captionLayout}
      formatters={{
        formatCaption: (date) => `${MESES_ES[date.getMonth()]} ${date.getFullYear()}`,
        formatWeekdayName: (date) => DIAS_ES[date.getDay()],
        formatMonthDropdown: (date) => MESES_CORTOS_ES[date.getMonth()],
        ...formatters,
      }}
      classNames={{
        root: cn("w-fit", defaultClassNames.root),
        months: cn(
          "relative flex flex-col gap-4 md:flex-row",
          defaultClassNames.months
        ),
        month: cn("flex w-full flex-col gap-3", defaultClassNames.month),
        nav: cn(
          "absolute inset-x-0 top-0 flex w-full items-center justify-between gap-1",
          defaultClassNames.nav
        ),
        button_previous: cn(
          buttonVariants({ variant: buttonVariant }),
          "h-[--cell-size] w-[--cell-size] select-none p-0 aria-disabled:opacity-50",
          defaultClassNames.button_previous
        ),
        button_next: cn(
          buttonVariants({ variant: buttonVariant }),
          "h-[--cell-size] w-[--cell-size] select-none p-0 aria-disabled:opacity-50",
          defaultClassNames.button_next
        ),
        month_caption: cn(
          "flex h-[--cell-size] w-full items-center justify-center px-[--cell-size] text-sm font-semibold text-zinc-800",
          defaultClassNames.month_caption
        ),
        dropdowns: cn(
          "flex h-[--cell-size] w-full items-center justify-center gap-1.5 text-xs font-medium",
          defaultClassNames.dropdowns
        ),
        dropdown_root: cn(
          "has-focus:border-ring border-input shadow-xs has-focus:ring-ring/50 has-focus:ring-[3px] relative rounded-md border",
          defaultClassNames.dropdown_root
        ),
        dropdown: cn(
          "bg-popover absolute inset-0 opacity-0",
          defaultClassNames.dropdown
        ),
        caption_label: cn(
          "select-none text-sm font-semibold text-zinc-800",
          defaultClassNames.caption_label
        ),
        table: "w-full border-collapse",
        weekdays: cn("flex border-b border-zinc-100", defaultClassNames.weekdays),
        weekday: cn(
          "text-zinc-400 flex-1 select-none rounded-md text-[10px] font-semibold uppercase tracking-widest py-1.5 text-center",
          defaultClassNames.weekday
        ),
        week: cn("mt-1 flex w-full", defaultClassNames.week),
        week_number_header: cn(
          "w-[--cell-size] select-none",
          defaultClassNames.week_number_header
        ),
        week_number: cn(
          "text-muted-foreground select-none text-[0.8rem]",
          defaultClassNames.week_number
        ),
        day: cn(
          "group/day relative aspect-square h-full w-full select-none p-0 text-center",
          defaultClassNames.day
        ),
        range_start: cn("rounded-l-md", defaultClassNames.range_start),
        range_middle: cn("rounded-none", defaultClassNames.range_middle),
        range_end: cn("rounded-r-md", defaultClassNames.range_end),
        today: cn(
          "rounded-md border border-indigo-200 bg-indigo-50",
          defaultClassNames.today
        ),
        outside: cn(
          "text-zinc-300 aria-selected:text-zinc-300",
          defaultClassNames.outside
        ),
        disabled: cn(
          "text-zinc-200 opacity-50",
          defaultClassNames.disabled
        ),
        hidden: cn("invisible", defaultClassNames.hidden),
        ...classNames,
      }}
      components={{
        Root: ({ className, rootRef, ...props }) => {
          return (
            <div
              data-slot="calendar"
              ref={rootRef}
              className={cn(className)}
              {...props}
            />
          )
        },
        Chevron: ({ className, orientation, ...props }) => {
          if (orientation === "left") {
            return (
              <ChevronLeftIcon className={cn("size-4", className)} {...props} />
            )
          }
          return (
            <ChevronRightIcon className={cn("size-4", className)} {...props} />
          )
        },
        DayButton: CalendarDayButton,
        WeekNumber: ({ children, ...props }) => {
          return (
            <td {...props}>
              <div className="flex size-[--cell-size] items-center justify-center text-center">
                {children}
              </div>
            </td>
          )
        },
        ...components,
      }}
      {...props}
    />
  )
}

export { Calendar, CalendarDayButton }
