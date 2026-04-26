import { useQuery } from '@tanstack/react-query'
import { subDays, format } from 'date-fns'
import { api } from '../../lib/api-client'
import { useDashboardOverview } from '../../hooks/useDashboard'
import { useTenantContext } from '../../context/TenantContext'
import { useAppointmentsByStatus, useAppointmentsTimeline, useBusiestHours, usePatientGrowth } from '../../hooks/useAdvancedReports'
import { MetricCards } from './MetricCards'
import { UpcomingList } from './UpcomingList'
import { AlertsList } from './AlertsList'
import { ChartCard } from '../../components/charts/ChartCard'
import { StatusDonutChart } from '../../components/charts/StatusDonutChart'
import { AppointmentsTimelineChart } from '../../components/charts/AppointmentsTimelineChart'
import { BusiestHoursHeatmap } from '../../components/charts/BusiestHoursHeatmap'
import { PatientGrowthChart } from '../../components/charts/PatientGrowthChart'

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
  const thirtyDaysAgo = format(subDays(new Date(), 30), 'yyyy-MM-dd')
  const { labels } = useTenantContext()

  const { data: overview, isLoading: overviewLoading } = useDashboardOverview()

  const { data, isLoading } = useQuery({
    queryKey: ['appointments', 'today', today],
    queryFn: () =>
      api.get<PagedResponse<Appointment>>(
        `/appointments?dateFrom=${today}&dateTo=${today}&pageSize=50`,
      ),
  })

  const { data: statusData, isLoading: statusLoading } = useAppointmentsByStatus(thirtyDaysAgo, today)
  const { data: timelineData, isLoading: timelineLoading } = useAppointmentsTimeline(thirtyDaysAgo, today)
  const { data: heatmapData, isLoading: heatmapLoading } = useBusiestHours(thirtyDaysAgo, today)
  const { data: growthData, isLoading: growthLoading } = usePatientGrowth(6)

  const appointments = data?.items ?? []

  return (
    <div className="space-y-6 animate-fade-in">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-500 mt-1">Visão geral das {labels.appointments.toLowerCase()} de hoje</p>
      </div>
      <MetricCards overview={overview} isLoading={overviewLoading} />
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <UpcomingList appointments={appointments} isLoading={isLoading} />
        <AlertsList appointments={appointments} isLoading={isLoading} />
      </div>

      {/* Advanced Charts Section */}
      <div>
        <h2 className="text-lg font-semibold text-gray-800 mb-4">Análise dos últimos 30 dias</h2>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <ChartCard title={`${labels.appointments} por status`} isLoading={statusLoading}>
            {statusData?.items && <StatusDonutChart data={statusData.items} />}
          </ChartCard>
          <ChartCard title={`Crescimento de ${labels.clients.toLowerCase()}`} isLoading={growthLoading}>
            {growthData?.points && <PatientGrowthChart data={growthData.points} />}
          </ChartCard>
          <ChartCard title="Linha do tempo de agendamentos" isLoading={timelineLoading} className="lg:col-span-2">
            {timelineData?.points && <AppointmentsTimelineChart data={timelineData.points} />}
          </ChartCard>
          <ChartCard title="Horários mais movimentados" isLoading={heatmapLoading} className="lg:col-span-2">
            {heatmapData?.slots && <BusiestHoursHeatmap data={heatmapData.slots} />}
          </ChartCard>
        </div>
      </div>
    </div>
  )
}
