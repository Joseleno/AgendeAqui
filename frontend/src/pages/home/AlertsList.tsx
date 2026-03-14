import { AlertTriangle, XCircle, Bell } from 'lucide-react'

interface Appointment {
  id: string
  clientName: string
  startTime: string
  status: string
}

interface AlertsListProps {
  appointments: Appointment[]
  isLoading: boolean
}

export function AlertsList({ appointments, isLoading }: AlertsListProps) {
  const noShows = appointments.filter((a) => a.status === 'NoShow')
  const cancelled = appointments.filter((a) => a.status === 'Cancelled')

  const alerts: { id: string; message: string; type: 'warning' | 'error' }[] = []

  for (const a of noShows) {
    alerts.push({
      id: a.id,
      message: `${a.clientName} faltou as ${a.startTime.slice(0, 5)}`,
      type: 'warning',
    })
  }

  for (const a of cancelled) {
    alerts.push({
      id: a.id,
      message: `${a.clientName} cancelou as ${a.startTime.slice(0, 5)}`,
      type: 'error',
    })
  }

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-5">
      <div className="flex items-center gap-2 mb-4">
        <Bell className="w-4.5 h-4.5 text-amber-500" />
        <h2 className="text-sm font-semibold text-gray-800">Alertas</h2>
      </div>
      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-12 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : alerts.length === 0 ? (
        <div className="text-center py-6">
          <Bell className="w-8 h-8 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum alerta</p>
        </div>
      ) : (
        <ul className="space-y-2">
          {alerts.map((alert) => (
            <li
              key={alert.id}
              className={`flex items-center gap-2.5 text-sm px-3.5 py-2.5 rounded-xl ${
                alert.type === 'error'
                  ? 'bg-red-50 text-red-700 border border-red-100'
                  : 'bg-amber-50 text-amber-700 border border-amber-100'
              }`}
            >
              {alert.type === 'error' ? (
                <XCircle className="w-4 h-4 shrink-0" />
              ) : (
                <AlertTriangle className="w-4 h-4 shrink-0" />
              )}
              <span>{alert.message}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
