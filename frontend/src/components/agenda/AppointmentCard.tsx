import { Clock, Scissors } from 'lucide-react'
import { getStatusStyle } from '../../lib/status-colors'
import type { Appointment } from '../../hooks/useAppointments'

interface AppointmentCardProps {
  appointment: Appointment
  onClick: (appointment: Appointment) => void
}

const borderColors: Record<string, string> = {
  Scheduled: 'border-l-yellow-400',
  Confirmed: 'border-l-green-400',
  InProgress: 'border-l-blue-400',
  Completed: 'border-l-gray-400',
  Cancelled: 'border-l-red-400',
  NoShow: 'border-l-gray-700',
}

export function AppointmentCard({ appointment, onClick }: AppointmentCardProps) {
  const style = getStatusStyle(appointment.status)
  const borderColor = borderColors[appointment.status] ?? 'border-l-gray-300'

  return (
    <button
      type="button"
      onClick={() => onClick(appointment)}
      className={`${style.bg} w-full text-left rounded-lg px-3 py-2 border-l-4 ${borderColor} border border-transparent hover:border-gray-300 hover:shadow-card-hover transition-all duration-200`}
    >
      <div className="flex items-center justify-between">
        <span className="flex items-center gap-1 text-xs font-semibold text-gray-600">
          <Clock className="w-3 h-3" />
          {appointment.startTime.slice(0, 5)} - {appointment.endTime.slice(0, 5)}
        </span>
        <span className={`${style.text} text-[10px] px-1.5 py-0.5 rounded-full bg-white/60 font-medium`}>
          {style.label}
        </span>
      </div>
      <p className="text-sm font-medium text-gray-800 truncate mt-0.5">
        {appointment.clientName}
      </p>
      <p className="text-xs text-gray-500 truncate flex items-center gap-1">
        <Scissors className="w-3 h-3" />
        {appointment.serviceName}
      </p>
    </button>
  )
}
