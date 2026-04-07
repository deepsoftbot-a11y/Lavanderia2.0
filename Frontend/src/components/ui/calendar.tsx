import * as React from "react"
import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react"
import { DayButton, DayPicker, getDefaultClassNames } from "react-day-picker"
import type { Matcher } from "react-day-picker"

import { cn } from "@/shared/utils/cn"
import { Button, buttonVariants } from "@/shared/components/ui/button"

// ─── Spanish constants ────────────────────────────────────────────
const MESES = [
  "Enero","Febrero","Marzo","Abril","Mayo","Junio",
  "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre",
]
const DIAS = ["Dom","Lun","Mar","Mié","Jue","Vie","Sáb"]

// ─── Day Button ─────────────────────────────────────────────────
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
      data-selected-single={modifiers.selected && !modifiers.range_start && !modifiers.range_end && !modifiers.range_middle}
      data-range-start={modifiers.range_start}
      data-range-end={modifiers.range_end}
      data-range-middle={modifiers.range_middle}
      data-today={modifiers.today}
      data-outside={day.outside}
      className={cn(
        // ── Base: zinc-50 cells, minimal padding ─────────────────
        "flex aspect-square h-auto w-full min-w-[36px] flex-col items-center justify-center gap-0 p-0 rounded-none font-normal leading-none " +
        "[&>span]:text-xs [&>span]:font-medium " +
        // ── Hover ───────────────────────────────────────────────
        "hover:bg-zinc-100 " +
        // ── Single selected: zinc-200 bg, zinc-800 text ──────────
        "data-[selected-single=true]:bg-zinc-200 data-[selected-single=true]:text-zinc-800 " +
        // ── Range start/end: indigo-600 bg ─────────────────────
        "data-[range-start=true]:bg-indigo-600 data-[range-start=true]:text-white " +
        "data-[range-end=true]:bg-indigo-600 data-[range-end=true]:text-white " +
        // ── Range middle: zinc-100 bg, zinc-600 text ─────────────
        "data-[range-middle=true]:bg-zinc-100 data-[range-middle=true]:text-zinc-700 " +
        // ── Today: indigo text, no bg ───────────────────────────
        "data-[today=true]:text-indigo-600 data-[today=true]:font-semibold " +
        // ── Outside: muted ──────────────────────────────────────
        "data-[outside=true]:text-zinc-300 data-[outside=true]:opacity-50 " +
        // ── Focus ───────────────────────────────────────────────
        "group-data-[focused=true]/day:ring-2 group-data-[focused=true]/day:ring-indigo-300",
        defaultClassNames.day,
        className
      )}
      {...props}
    />
  )
}

// ─── Disabled: future dates ─────────────────────────────────────
const tomorrow = new Date()
tomorrow.setHours(0, 0, 0, 0)
tomorrow.setDate(tomorrow.getDate() + 1)
const DISABLED_FUTURE: Matcher = { after: tomorrow }

// ─── Calendar ────────────────────────────────────────────────────
function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  captionLayout = "label",
  buttonVariant = "ghost",
  formatters,
  components,
  numberOfMonths = 1,
  ...props
}: React.ComponentProps<typeof DayPicker> & {
  buttonVariant?: React.ComponentProps<typeof Button>["variant"]
}) {
  const defaultClassNames = getDefaultClassNames()

  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      disabled={DISABLED_FUTURE}
      numberOfMonths={numberOfMonths}
      className={cn(
        // ── Outer shell: no bg, borders structural ───────────────
        "p-3 [--cell-size:36px] text-zinc-800",
        String.raw`rtl:**:[.rdp-button\_next>svg]:rotate-180`,
        String.raw`rtl:**:[.rdp-button\_previous>svg]:rotate-180`,
        className
      )}
      captionLayout={captionLayout}
      formatters={{
        // ── Spanish captions ───────────────────────────────────
        formatCaption: (date) => `${MESES[date.getMonth()]} ${date.getFullYear()}`,
        formatWeekdayName: (date) => DIAS[date.getDay()],
        formatMonthDropdown: (date) => MESES[date.getMonth()].slice(0, 3),
        ...formatters,
      }}
      classNames={{
        root: cn("w-fit", defaultClassNames.root),
        months: cn(
          "relative flex flex-col gap-0 md:flex-row md:gap-3",
          defaultClassNames.months
        ),
        month: cn("flex w-full flex-col gap-3", defaultClassNames.month),
        // ── Navigation: border-bottom instead of absolute ───────
        nav: cn(
          "flex items-center justify-between border-b border-zinc-100 pb-2.5 mb-1.5 w-full",
          defaultClassNames.nav
        ),
        button_previous: cn(
          buttonVariants({ variant: buttonVariant }),
          "h-8 w-8 p-0 text-zinc-400 hover:text-zinc-700 hover:bg-zinc-100 aria-disabled:opacity-30",
          defaultClassNames.button_previous
        ),
        button_next: cn(
          buttonVariants({ variant: buttonVariant }),
          "h-8 w-8 p-0 text-zinc-400 hover:text-zinc-700 hover:bg-zinc-100 aria-disabled:opacity-30",
          defaultClassNames.button_next
        ),
        // ── Caption: tight, label-style ─────────────────────────
        month_caption: cn(
          "flex items-center h-8 px-1 text-sm font-semibold text-zinc-800",
          defaultClassNames.month_caption
        ),
        caption_label: cn(
          "text-sm font-semibold text-zinc-800",
          defaultClassNames.caption_label
        ),
        dropdowns: cn(
          "flex items-center gap-1 text-xs font-medium",
          defaultClassNames.dropdowns
        ),
        dropdown_root: cn(
          "has-focus:border-ring border border-zinc-200 rounded-md",
          defaultClassNames.dropdown_root
        ),
        dropdown: cn("bg-popover absolute inset-0 opacity-0", defaultClassNames.dropdown),
        table: "w-full border-collapse",
        // ── Weekday row: uppercase label style ───────────────────
        weekdays: cn("flex border-b border-zinc-100", defaultClassNames.weekdays),
        weekday: cn(
          "text-[10px] font-semibold uppercase tracking-widest text-zinc-400 flex-1 py-1.5 text-center",
          defaultClassNames.weekday
        ),
        week: cn("mt-0.5 flex w-full", defaultClassNames.week),
        week_number_header: cn("w-[36px] select-none", defaultClassNames.week_number_header),
        week_number: cn("text-muted-foreground select-none text-[0.8rem]", defaultClassNames.week_number),
        day: cn(
          "group/day relative aspect-square h-full w-full select-none p-0 text-center",
          defaultClassNames.day
        ),
        range_start: cn("rounded-l-md", defaultClassNames.range_start),
        range_middle: cn("rounded-none", defaultClassNames.range_middle),
        range_end: cn("rounded-r-md", defaultClassNames.range_end),
        today: cn("text-indigo-600 font-semibold", defaultClassNames.today),
        outside: cn("text-zinc-300 aria-selected:text-zinc-300", defaultClassNames.outside),
        disabled: cn("text-zinc-300 opacity-50", defaultClassNames.disabled),
        hidden: cn("invisible", defaultClassNames.hidden),
        ...classNames,
      }}
      components={{
        Root: ({ className, rootRef, ...p }) => (
          <div data-slot="calendar" ref={rootRef} className={cn(className)} {...p} />
        ),
        Chevron: ({ className, orientation, ...p }) => {
          if (orientation === "left")
            return <ChevronLeftIcon className={cn("size-4", className)} {...p} />
          return <ChevronRightIcon className={cn("size-4", className)} {...p} />
        },
        DayButton: CalendarDayButton,
        WeekNumber: ({ children, ...p }) => (
          <td {...p}>
            <div className="flex size-[36px] items-center justify-center text-center">
              {children}
            </div>
          </td>
        ),
        ...components,
      }}
      {...props}
    />
  )
}

export { Calendar, CalendarDayButton }
