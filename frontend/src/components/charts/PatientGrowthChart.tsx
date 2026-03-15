import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import type { GrowthPoint } from '../../hooks/useAdvancedReports'

interface PatientGrowthChartProps {
  data: GrowthPoint[]
}

export function PatientGrowthChart({ data }: PatientGrowthChartProps) {
  const chartData = data.map((d) => ({
    month: d.month,
    'Total pacientes': d.totalClients,
    'Novos': d.newClients,
  }))

  return (
    <ResponsiveContainer width="100%" height={300}>
      <AreaChart data={chartData} margin={{ top: 5, right: 5, bottom: 5, left: -10 }}>
        <defs>
          <linearGradient id="gradGrowth" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
            <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
        <XAxis dataKey="month" tick={{ fontSize: 11 }} />
        <YAxis tick={{ fontSize: 11 }} allowDecimals={false} />
        <Tooltip />
        <Area
          type="monotone"
          dataKey="Total pacientes"
          stroke="#8b5cf6"
          fill="url(#gradGrowth)"
          strokeWidth={2}
          dot={{ fill: '#8b5cf6', r: 3 }}
        />
        <Area
          type="monotone"
          dataKey="Novos"
          stroke="#a78bfa"
          fill="none"
          strokeWidth={1.5}
          strokeDasharray="4 4"
        />
      </AreaChart>
    </ResponsiveContainer>
  )
}
