import { addDays, format, startOfWeek } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import type { Appointment } from '../../hooks/useAppointments'
import { AppointmentCard } from './AppointmentCard'

interface WeekViewProps {
  currentDate: Date
  appointments: Appointment[]
  onSelect: (appointment: Appointment) => void
}

export function WeekView({ currentDate, appointments, onSelect }: WeekViewProps) {
  const weekStart = startOfWeek(currentDate, { weekStartsOn: 1 })
  const days = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))

  function getForDay(day: Date) {
    const iso = format(day, 'yyyy-MM-dd')
    return appointments.filter((a) => a.date === iso)
  }

  return (
    <div className="grid grid-cols-7 gap-2">
      {days.map((day) => {
        const dayAppts = getForDay(day)
        const isToday = format(day, 'yyyy-MM-dd') === format(new Date(), 'yyyy-MM-dd')
        return (
          <div key={day.toISOString()} className="min-h-[200px]">
            <div
              className={`text-center text-xs font-medium py-1 rounded-t-lg ${
                isToday ? 'bg-brand-600 text-white' : 'bg-gray-100 text-gray-600'
              }`}
            >
              <div>{format(day, 'EEE', { locale: ptBR })}</div>
              <div className="text-lg font-bold">{format(day, 'd')}</div>
            </div>
            <div className="space-y-1 mt-1">
              {dayAppts
                .sort((a, b) => a.startTime.localeCompare(b.startTime))
                .slice(0, 5)
                .map((a) => (
                  <AppointmentCard key={a.id} appointment={a} onClick={onSelect} />
                ))}
              {dayAppts.length > 5 && (
                <p className="text-[10px] text-gray-400 text-center">
                  +{dayAppts.length - 5} mais
                </p>
              )}
            </div>
          </div>
        )
      })}
    </div>
  )
}
