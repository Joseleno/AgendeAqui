import { Clock, CalendarDays } from 'lucide-react'
import { getStatusStyle } from '../../lib/status-colors'

interface Appointment {
  id: string
  professionalName: string
  serviceName: string
  clientName: string
  startTime: string
  endTime: string
  status: string
}

interface UpcomingListProps {
  appointments: Appointment[]
  isLoading: boolean
}

export function UpcomingList({ appointments, isLoading }: UpcomingListProps) {
  const upcoming = appointments
    .filter((a) => a.status !== 'Cancelled' && a.status !== 'Completed' && a.status !== 'NoShow')
    .sort((a, b) => a.startTime.localeCompare(b.startTime))

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-5">
      <div className="flex items-center gap-2 mb-4">
        <CalendarDays className="w-4.5 h-4.5 text-brand-600" />
        <h2 className="text-sm font-semibold text-gray-800">Proximos agendamentos</h2>
      </div>
      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-12 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : upcoming.length === 0 ? (
        <div className="text-center py-6">
          <Clock className="w-8 h-8 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum agendamento pendente hoje</p>
        </div>
      ) : (
        <ul className="space-y-2">
          {upcoming.slice(0, 8).map((a) => {
            const style = getStatusStyle(a.status)
            return (
              <li key={a.id} className="flex items-center justify-between p-2.5 rounded-xl hover:bg-gray-50 transition-colors">
                <div className="min-w-0">
                  <p className="font-medium text-gray-800 text-sm truncate">
                    {a.startTime.slice(0, 5)} — {a.clientName}
                  </p>
                  <p className="text-xs text-gray-500 truncate mt-0.5">
                    {a.serviceName} &middot; {a.professionalName}
                  </p>
                </div>
                <span className={`${style.bg} ${style.text} text-xs px-2.5 py-1 rounded-full shrink-0 ml-2 font-medium`}>
                  {style.label}
                </span>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}
