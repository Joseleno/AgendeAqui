import type { Appointment } from '../../hooks/useAppointments'
import { AppointmentCard } from './AppointmentCard'

interface DayViewProps {
  appointments: Appointment[]
  onSelect: (appointment: Appointment) => void
}

const HOURS = Array.from({ length: 13 }, (_, i) => i + 7) // 07:00 - 19:00

export function DayView({ appointments, onSelect }: DayViewProps) {
  function getAppointmentsForHour(hour: number) {
    const hStr = String(hour).padStart(2, '0')
    return appointments.filter((a) => a.startTime.startsWith(hStr + ':'))
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      {HOURS.map((hour) => {
        const hourAppts = getAppointmentsForHour(hour)
        return (
          <div key={hour} className="flex border-b border-gray-100 last:border-b-0 min-h-[56px]">
            <div className="w-16 shrink-0 py-2 px-2 text-xs text-gray-400 text-right border-r border-gray-100">
              {String(hour).padStart(2, '0')}:00
            </div>
            <div className="flex-1 p-1 space-y-1">
              {hourAppts.map((a) => (
                <AppointmentCard key={a.id} appointment={a} onClick={onSelect} />
              ))}
            </div>
          </div>
        )
      })}
    </div>
  )
}
