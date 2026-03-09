interface Appointment {
  status: string
  notes: string | null
}

interface MetricCardsProps {
  appointments: Appointment[]
  isLoading: boolean
}

export function MetricCards({ appointments, isLoading }: MetricCardsProps) {
  const total = appointments.length
  const confirmed = appointments.filter(
    (a) => a.status === 'Confirmed' || a.status === 'InProgress' || a.status === 'Completed',
  ).length
  const cancelled = appointments.filter((a) => a.status === 'Cancelled').length

  const cards = [
    { label: 'Total hoje', value: total, color: 'text-indigo-600', bg: 'bg-indigo-50' },
    { label: 'Confirmados', value: confirmed, color: 'text-green-600', bg: 'bg-green-50' },
    { label: 'Cancelados', value: cancelled, color: 'text-red-600', bg: 'bg-red-50' },
  ]

  return (
    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
      {cards.map((card) => (
        <div
          key={card.label}
          className={`${card.bg} rounded-xl p-4 border border-gray-100`}
        >
          <p className="text-sm text-gray-600">{card.label}</p>
          <p className={`text-2xl font-bold ${card.color} mt-1`}>
            {isLoading ? '-' : card.value}
          </p>
        </div>
      ))}
    </div>
  )
}
