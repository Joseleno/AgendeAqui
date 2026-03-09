import { getStatusStyle } from '../../lib/status-colors'
import type { Appointment } from '../../hooks/useAppointments'

interface AppointmentCardProps {
  appointment: Appointment
  onClick: (appointment: Appointment) => void
}

export function AppointmentCard({ appointment, onClick }: AppointmentCardProps) {
  const style = getStatusStyle(appointment.status)

  return (
    <button
      type="button"
      onClick={() => onClick(appointment)}
      className={`${style.bg} w-full text-left rounded-lg px-3 py-2 border border-transparent hover:border-gray-300 transition-colors`}
    >
      <div className="flex items-center justify-between">
        <span className={`text-xs font-semibold ${style.text}`}>
          {appointment.startTime.slice(0, 5)} - {appointment.endTime.slice(0, 5)}
        </span>
        <span className={`${style.text} text-[10px] px-1.5 py-0.5 rounded-full bg-white/60`}>
          {style.label}
        </span>
      </div>
      <p className="text-sm font-medium text-gray-800 truncate mt-0.5">
        {appointment.clientName}
      </p>
      <p className="text-xs text-gray-500 truncate">
        {appointment.serviceName}
      </p>
    </button>
  )
}
