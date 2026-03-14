import { useQuery } from '@tanstack/react-query'
import { api } from '../../lib/api-client'
import { MetricCards } from './MetricCards'
import { UpcomingList } from './UpcomingList'
import { AlertsList } from './AlertsList'

interface Appointment {
  id: string
  professionalName: string
  serviceName: string
  clientName: string
  date: string
  startTime: string
  endTime: string
  status: string
  notes: string | null
}

interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

function todayISO() {
  return new Date().toLocaleDateString('sv-SE')
}

export function HomePage() {
  const today = todayISO()

  const { data, isLoading } = useQuery({
    queryKey: ['appointments', 'today', today],
    queryFn: () =>
      api.get<PagedResponse<Appointment>>(
        `/appointments?dateFrom=${today}&dateTo=${today}&pageSize=50`,
      ),
  })

  const appointments = data?.items ?? []

  return (
    <div className="space-y-6 animate-fade-in">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-500 mt-1">Visão geral dos agendamentos de hoje</p>
      </div>
      <MetricCards appointments={appointments} isLoading={isLoading} />
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <UpcomingList appointments={appointments} isLoading={isLoading} />
        <AlertsList appointments={appointments} isLoading={isLoading} />
      </div>
    </div>
  )
}
