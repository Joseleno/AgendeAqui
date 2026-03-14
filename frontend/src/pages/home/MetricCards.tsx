import { CalendarCheck, CalendarDays, Stethoscope, Users, XCircle, AlertTriangle } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import type { DashboardOverview } from '../../hooks/useDashboard'

interface MetricCardsProps {
  overview?: DashboardOverview
  isLoading: boolean
}

export function MetricCards({ overview, isLoading }: MetricCardsProps) {
  const cards: { label: string; value: number; icon: LucideIcon; iconColor: string; bgGradient: string }[] = [
    {
      label: 'Agendamentos hoje',
      value: overview?.appointmentsToday ?? 0,
      icon: CalendarCheck,
      iconColor: 'text-brand-600',
      bgGradient: 'from-brand-50 to-white',
    },
    {
      label: 'Agendamentos semana',
      value: overview?.appointmentsThisWeek ?? 0,
      icon: CalendarDays,
      iconColor: 'text-blue-600',
      bgGradient: 'from-blue-50 to-white',
    },
    {
      label: 'Profissionais ativos',
      value: overview?.activeProfessionals ?? 0,
      icon: Stethoscope,
      iconColor: 'text-purple-600',
      bgGradient: 'from-purple-50 to-white',
    },
    {
      label: 'Pacientes registrados',
      value: overview?.registeredClients ?? 0,
      icon: Users,
      iconColor: 'text-emerald-600',
      bgGradient: 'from-emerald-50 to-white',
    },
    {
      label: 'Cancelados hoje',
      value: overview?.cancelledToday ?? 0,
      icon: XCircle,
      iconColor: 'text-red-500',
      bgGradient: 'from-red-50 to-white',
    },
    {
      label: 'Faltas hoje',
      value: overview?.noShowToday ?? 0,
      icon: AlertTriangle,
      iconColor: 'text-amber-500',
      bgGradient: 'from-amber-50 to-white',
    },
  ]

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
      {cards.map((card) => (
        <div
          key={card.label}
          className={`bg-gradient-to-br ${card.bgGradient} rounded-2xl p-5 border border-gray-100 shadow-card hover:shadow-card-hover transition-all duration-300`}
        >
          <div className="flex items-center justify-between">
            <p className="text-xs font-medium text-gray-500">{card.label}</p>
            <card.icon className={`w-4.5 h-4.5 ${card.iconColor}`} />
          </div>
          <p className="text-2xl font-bold text-gray-900 mt-2 tabular-nums">
            {isLoading ? (
              <span className="inline-block w-8 h-7 bg-gray-200 rounded-lg animate-pulse" />
            ) : (
              card.value
            )}
          </p>
        </div>
      ))}
    </div>
  )
}
