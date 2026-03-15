import { CalendarDays } from 'lucide-react'
import { getStatusStyle } from '../../lib/status-colors'
import { EmptyState } from '../../components/ui/EmptyState'
import { SkeletonText } from '../../components/ui/Skeleton'

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
        <h2 className="text-sm font-semibold text-gray-800">Próximos agendamentos</h2>
      </div>
      {isLoading ? (
        <div className="space-y-4 py-2">
          {[1, 2, 3].map((i) => (
            <SkeletonText key={i} lines={2} />
          ))}
        </div>
      ) : upcoming.length === 0 ? (
        <EmptyState
          icon={CalendarDays}
          title="Nenhum agendamento pendente"
          subtitle="Não há agendamentos pendentes para hoje"
        />
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
