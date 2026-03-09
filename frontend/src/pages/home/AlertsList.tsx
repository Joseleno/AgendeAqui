interface Appointment {
  id: string
  clientName: string
  startTime: string
  status: string
}

interface AlertsListProps {
  appointments: Appointment[]
}

export function AlertsList({ appointments }: AlertsListProps) {
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
    <div className="bg-white rounded-xl border border-gray-200 p-4">
      <h2 className="text-sm font-semibold text-gray-700 mb-3">Alertas</h2>
      {alerts.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum alerta</p>
      ) : (
        <ul className="space-y-2">
          {alerts.map((alert) => (
            <li
              key={alert.id}
              className={`text-sm px-3 py-2 rounded-lg ${
                alert.type === 'error'
                  ? 'bg-red-50 text-red-700'
                  : 'bg-yellow-50 text-yellow-700'
              }`}
            >
              {alert.message}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
