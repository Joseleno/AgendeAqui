import { CalendarDays } from 'lucide-react'

interface DateRangePickerProps {
  dateFrom: string
  dateTo: string
  onDateFromChange: (value: string) => void
  onDateToChange: (value: string) => void
}

export function DateRangePicker({ dateFrom, dateTo, onDateFromChange, onDateToChange }: DateRangePickerProps) {
  return (
    <div className="flex items-center gap-2">
      <div className="relative">
        <CalendarDays className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          type="date"
          value={dateFrom}
          onChange={(e) => onDateFromChange(e.target.value)}
          aria-label="Data inicial"
          className="rounded-xl border border-gray-200 bg-gray-50/50 pl-8 pr-3 py-2 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
        />
      </div>
      <span className="text-sm text-gray-400 font-medium">até</span>
      <div className="relative">
        <CalendarDays className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          type="date"
          value={dateTo}
          onChange={(e) => onDateToChange(e.target.value)}
          aria-label="Data final"
          className="rounded-xl border border-gray-200 bg-gray-50/50 pl-8 pr-3 py-2 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
        />
      </div>
    </div>
  )
}
