import {
  startOfMonth,
  endOfMonth,
  startOfWeek,
  endOfWeek,
  addDays,
  format,
  isSameMonth,
} from 'date-fns'
import { ptBR } from 'date-fns/locale'
import type { Appointment } from '../../hooks/useAppointments'
import { statusColors } from '../../lib/status-colors'

interface MonthViewProps {
  currentDate: Date
  appointments: Appointment[]
  onDayClick: (date: Date) => void
}

export function MonthView({ currentDate, appointments, onDayClick }: MonthViewProps) {
  const monthStart = startOfMonth(currentDate)
  const monthEnd = endOfMonth(currentDate)
  const calStart = startOfWeek(monthStart, { weekStartsOn: 1 })
  const calEnd = endOfWeek(monthEnd, { weekStartsOn: 1 })

  const days: Date[] = []
  let day = calStart
  while (day <= calEnd) {
    days.push(day)
    day = addDays(day, 1)
  }

  const weekdays = Array.from({ length: 7 }, (_, i) =>
    format(addDays(startOfWeek(new Date(), { weekStartsOn: 1 }), i), 'EEE', { locale: ptBR }),
  )

  function getForDay(d: Date) {
    const iso = format(d, 'yyyy-MM-dd')
    return appointments.filter((a) => a.date === iso)
  }

  const todayStr = format(new Date(), 'yyyy-MM-dd')

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="grid grid-cols-7">
        {weekdays.map((wd) => (
          <div key={wd} className="text-center text-xs font-medium text-gray-500 py-2 border-b border-gray-100">
            {wd}
          </div>
        ))}
      </div>
      <div className="grid grid-cols-7">
        {days.map((d) => {
          const dayStr = format(d, 'yyyy-MM-dd')
          const isToday = dayStr === todayStr
          const inMonth = isSameMonth(d, currentDate)
          const dayAppts = getForDay(d)
          return (
            <button
              type="button"
              key={dayStr}
              onClick={() => onDayClick(d)}
              className={`min-h-[72px] p-1 border-b border-r border-gray-100 text-left hover:bg-gray-50 transition-colors ${
                !inMonth ? 'opacity-40' : ''
              }`}
            >
              <span
                className={`text-xs font-medium inline-block w-6 h-6 leading-6 text-center rounded-full ${
                  isToday ? 'bg-brand-600 text-white' : 'text-gray-700'
                }`}
              >
                {format(d, 'd')}
              </span>
              <div className="flex flex-wrap gap-0.5 mt-0.5">
                {dayAppts.slice(0, 4).map((a) => {
                  const color = statusColors[a.status]?.bg ?? 'bg-gray-300'
                  return (
                    <span
                      key={a.id}
                      className={`${color} w-2 h-2 rounded-full`}
                    />
                  )
                })}
                {dayAppts.length > 4 && (
                  <span className="text-[9px] text-gray-400">+{dayAppts.length - 4}</span>
                )}
              </div>
            </button>
          )
        })}
      </div>
    </div>
  )
}
