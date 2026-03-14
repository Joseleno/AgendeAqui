import { CalendarCheck, CheckCircle, XCircle } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

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

  const cards: { label: string; value: number; icon: LucideIcon; iconColor: string; bgGradient: string }[] = [
    {
      label: 'Total hoje',
      value: total,
      icon: CalendarCheck,
      iconColor: 'text-brand-600',
      bgGradient: 'from-brand-50 to-white',
    },
    {
      label: 'Confirmados',
      value: confirmed,
      icon: CheckCircle,
      iconColor: 'text-emerald-600',
      bgGradient: 'from-emerald-50 to-white',
    },
    {
      label: 'Cancelados',
      value: cancelled,
      icon: XCircle,
      iconColor: 'text-red-500',
      bgGradient: 'from-red-50 to-white',
    },
  ]

  return (
    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
      {cards.map((card) => (
        <div
          key={card.label}
          className={`bg-gradient-to-br ${card.bgGradient} rounded-2xl p-5 border border-gray-100 shadow-card hover:shadow-card-hover transition-all duration-300`}
        >
          <div className="flex items-center justify-between">
            <p className="text-sm font-medium text-gray-500">{card.label}</p>
            <card.icon className={`w-5 h-5 ${card.iconColor}`} />
          </div>
          <p className="text-3xl font-bold text-gray-900 mt-2 tabular-nums">
            {isLoading ? (
              <span className="inline-block w-10 h-8 bg-gray-200 rounded-lg animate-pulse" />
            ) : (
              card.value
            )}
          </p>
        </div>
      ))}
    </div>
  )
}
