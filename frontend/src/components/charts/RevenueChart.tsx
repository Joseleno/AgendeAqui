import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts'
import type { ServiceRevenue } from '../../hooks/useReports'

interface RevenueChartProps {
  data: ServiceRevenue[]
}

export function RevenueChart({ data }: RevenueChartProps) {
  const chartData = data.map((d) => ({
    name: d.serviceName,
    receita: d.totalRevenue,
    agendamentos: d.appointmentCount,
  }))

  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={chartData}>
        <XAxis dataKey="name" tick={{ fontSize: 12 }} />
        <YAxis tick={{ fontSize: 12 }} />
        <Tooltip
          formatter={(value, name) =>
            name === 'receita' ? `R$ ${Number(value).toFixed(2)}` : value
          }
        />
        <Bar dataKey="receita" fill="#6366f1" name="Receita" />
      </BarChart>
    </ResponsiveContainer>
  )
}
