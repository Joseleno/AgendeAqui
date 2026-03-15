import { useMemo } from 'react'
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts'
import type { StatusCount } from '../../hooks/useAdvancedReports'

interface StatusDonutChartProps {
  data: StatusCount[]
}

const STATUS_COLORS: Record<string, string> = {
  Scheduled: '#eab308',
  Confirmed: '#22c55e',
  InProgress: '#3b82f6',
  Completed: '#6b7280',
  Cancelled: '#ef4444',
  NoShow: '#1f2937',
}

const STATUS_LABELS: Record<string, string> = {
  Scheduled: 'Agendado',
  Confirmed: 'Confirmado',
  InProgress: 'Em andamento',
  Completed: 'Concluído',
  Cancelled: 'Cancelado',
  NoShow: 'Faltou',
}

export function StatusDonutChart({ data }: StatusDonutChartProps) {
  const { chartData, total } = useMemo(() => {
    const items = data.map((d) => ({
      name: STATUS_LABELS[d.status] ?? d.status,
      value: d.count,
      fill: STATUS_COLORS[d.status] ?? '#9ca3af',
    }))
    return { chartData: items, total: data.reduce((sum, d) => sum + d.count, 0) }
  }, [data])

  return (
    <div className="flex items-center gap-6">
      <div className="relative w-48 h-48 shrink-0">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              innerRadius={55}
              outerRadius={80}
              paddingAngle={2}
              dataKey="value"
              stroke="none"
            >
              {chartData.map((entry, i) => (
                <Cell key={i} fill={entry.fill} />
              ))}
            </Pie>
            <Tooltip
              formatter={(value) => [Number(value), 'Agendamentos']}
            />
          </PieChart>
        </ResponsiveContainer>
        <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
          <div className="text-center">
            <p className="text-2xl font-bold text-gray-900">{total}</p>
            <p className="text-xs text-gray-500">Total</p>
          </div>
        </div>
      </div>
      <div className="flex flex-col gap-2">
        {chartData.map((entry) => (
          <div key={entry.name} className="flex items-center gap-2 text-sm">
            <div
              className="w-3 h-3 rounded-full shrink-0"
              style={{ backgroundColor: entry.fill }}
            />
            <span className="text-gray-600">{entry.name}</span>
            <span className="font-semibold text-gray-800 ml-auto tabular-nums">{entry.value}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
