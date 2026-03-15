import { useMemo, useId } from 'react'
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import type { TimelinePoint } from '../../hooks/useAdvancedReports'

interface AppointmentsTimelineChartProps {
  data: TimelinePoint[]
}

export function AppointmentsTimelineChart({ data }: AppointmentsTimelineChartProps) {
  const uid = useId().replace(/:/g, '')
  const chartData = useMemo(() => data.map((d) => ({
    period: d.period.length > 7 ? d.period.slice(5) : d.period,
    Concluídos: d.completed,
    Agendados: d.scheduled,
    Cancelados: d.cancelled,
    Faltou: d.noShow,
  })), [data])

  return (
    <ResponsiveContainer width="100%" height={300}>
      <AreaChart data={chartData} margin={{ top: 5, right: 5, bottom: 5, left: -10 }}>
        <defs>
          <linearGradient id={`gradCompleted-${uid}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#22c55e" stopOpacity={0.3} />
            <stop offset="95%" stopColor="#22c55e" stopOpacity={0} />
          </linearGradient>
          <linearGradient id={`gradScheduled-${uid}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
            <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
        <XAxis dataKey="period" tick={{ fontSize: 11 }} />
        <YAxis tick={{ fontSize: 11 }} allowDecimals={false} />
        <Tooltip />
        <Area type="monotone" dataKey="Concluídos" stroke="#22c55e" fill={`url(#gradCompleted-${uid})`} strokeWidth={2} />
        <Area type="monotone" dataKey="Agendados" stroke="#3b82f6" fill={`url(#gradScheduled-${uid})`} strokeWidth={2} />
        <Area type="monotone" dataKey="Cancelados" stroke="#ef4444" fill="none" strokeWidth={1.5} strokeDasharray="4 4" />
        <Area type="monotone" dataKey="Faltou" stroke="#f59e0b" fill="none" strokeWidth={1.5} strokeDasharray="4 4" />
      </AreaChart>
    </ResponsiveContainer>
  )
}
