import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Legend } from 'recharts'
import type { ProfessionalAttendance } from '../../hooks/useReports'

interface AttendanceChartProps {
  data: ProfessionalAttendance[]
}

export function AttendanceChart({ data }: AttendanceChartProps) {
  const chartData = data.map((d) => ({
    name: d.professionalName,
    Concluidos: d.completed,
    Cancelados: d.cancelled,
    Faltou: d.noShow,
  }))

  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={chartData} layout="vertical" margin={{ left: 20 }}>
        <XAxis type="number" />
        <YAxis type="category" dataKey="name" width={120} tick={{ fontSize: 12 }} />
        <Tooltip />
        <Legend />
        <Bar dataKey="Concluidos" fill="#22c55e" stackId="a" />
        <Bar dataKey="Cancelados" fill="#ef4444" stackId="a" />
        <Bar dataKey="Faltou" fill="#6b7280" stackId="a" />
      </BarChart>
    </ResponsiveContainer>
  )
}
