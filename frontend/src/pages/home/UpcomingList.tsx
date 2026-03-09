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
    <div className="bg-white rounded-xl border border-gray-200 p-4">
      <h2 className="text-sm font-semibold text-gray-700 mb-3">Proximos agendamentos</h2>
      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : upcoming.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum agendamento pendente hoje</p>
      ) : (
        <ul className="space-y-2">
          {upcoming.slice(0, 8).map((a) => {
            const style = getStatusStyle(a.status)
            return (
              <li key={a.id} className="flex items-center justify-between text-sm">
                <div className="min-w-0">
                  <p className="font-medium text-gray-800 truncate">
                    {a.startTime.slice(0, 5)} — {a.clientName}
                  </p>
                  <p className="text-xs text-gray-500 truncate">
                    {a.serviceName} &middot; {a.professionalName}
                  </p>
                </div>
                <span className={`${style.bg} ${style.text} text-xs px-2 py-0.5 rounded-full shrink-0 ml-2`}>
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
