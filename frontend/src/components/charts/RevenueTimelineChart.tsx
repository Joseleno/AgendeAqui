import { useMemo, useId } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'
import type { RevenuePoint } from '../../hooks/useAdvancedReports'
import { formatCurrency } from '../../lib/format'

interface RevenueTimelineChartProps {
  data: RevenuePoint[]
}

export function RevenueTimelineChart({ data }: RevenueTimelineChartProps) {
  const uid = useId().replace(/:/g, '')
  const chartData = useMemo(() => data.map((d) => ({
    month: d.month,
    Receita: d.revenue,
    Atendimentos: d.appointmentCount,
  })), [data])

  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={chartData} margin={{ top: 5, right: 5, bottom: 5, left: -10 }}>
        <defs>
          <linearGradient id={`gradRevenue-${uid}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="#14b8a6" />
            <stop offset="100%" stopColor="#0d9488" />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
        <XAxis dataKey="month" tick={{ fontSize: 11 }} />
        <YAxis
          tick={{ fontSize: 11 }}
          tickFormatter={(v) => `R$${v >= 1000 ? `${(v / 1000).toFixed(0)}k` : v}`}
        />
        <Tooltip
          formatter={(value, name) =>
            name === 'Receita' ? [formatCurrency(Number(value)), String(name)] : [Number(value), String(name)]
          }
        />
        <Bar dataKey="Receita" fill={`url(#gradRevenue-${uid})`} radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  )
}
