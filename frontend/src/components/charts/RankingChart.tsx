import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Legend } from 'recharts'
import type { ProfessionalRanking } from '../../hooks/useDashboard'

interface RankingChartProps {
  data: ProfessionalRanking[]
}

export function RankingChart({ data }: RankingChartProps) {
  const chartData = data.map((d) => ({
    name: d.name,
    Concluídos: d.completed,
    Cancelados: d.cancelled,
    Faltou: d.noShow,
  }))

  return (
    <ResponsiveContainer width="100%" height={Math.min(600, Math.max(200, data.length * 40))}>
      <BarChart data={chartData} layout="vertical" margin={{ left: 20 }}>
        <XAxis type="number" />
        <YAxis type="category" dataKey="name" width={120} tick={{ fontSize: 12 }} />
        <Tooltip />
        <Legend />
        <Bar dataKey="Concluídos" fill="#22c55e" stackId="a" />
        <Bar dataKey="Cancelados" fill="#ef4444" stackId="a" />
        <Bar dataKey="Faltou" fill="#6b7280" stackId="a" />
      </BarChart>
    </ResponsiveContainer>
  )
}
