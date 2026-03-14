import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { BarChart3, CheckCircle, XCircle, AlertTriangle, DollarSign } from 'lucide-react'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { useMyStats } from '../../hooks/useMyCalendar'

export function MinhasStatsPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))

  const { data, isLoading } = useMyStats(dateFrom, dateTo)

  const completionRate = data && data.total > 0
    ? Math.round((data.completed / data.total) * 100)
    : 0

  return (
    <div className="space-y-4 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Minhas Estatísticas</h1>
        <DateRangePicker dateFrom={dateFrom} dateTo={dateTo} onDateFromChange={setDateFrom} onDateToChange={setDateTo} />
      </div>

      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="h-24 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : data ? (
        <>
          <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
            <StatCard label="Total" value={data.total} icon={BarChart3} iconColor="text-brand-600" bg="bg-brand-50" />
            <StatCard label="Concluídos" value={data.completed} icon={CheckCircle} iconColor="text-green-600" bg="bg-green-50" />
            <StatCard label="Cancelados" value={data.cancelled} icon={XCircle} iconColor="text-red-500" bg="bg-red-50" />
            <StatCard label="Faltas" value={data.noShow} icon={AlertTriangle} iconColor="text-amber-500" bg="bg-amber-50" />
            <StatCard label="Receita" value={`R$ ${data.revenue.toFixed(2)}`} icon={DollarSign} iconColor="text-emerald-600" bg="bg-emerald-50" />
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-700">Taxa de conclusão</span>
              <span className={`text-sm font-semibold ${completionRate >= 80 ? 'text-green-600' : completionRate >= 50 ? 'text-yellow-600' : 'text-red-600'}`}>
                {completionRate}%
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-3">
              <div
                className={`h-3 rounded-full transition-all duration-500 ${completionRate >= 80 ? 'bg-green-500' : completionRate >= 50 ? 'bg-yellow-500' : 'bg-red-500'}`}
                style={{ width: `${completionRate}%` }}
              />
            </div>
          </div>
        </>
      ) : (
        <p className="text-sm text-gray-400">Selecione um período</p>
      )}
    </div>
  )
}

function StatCard({ label, value, icon: Icon, iconColor, bg }: {
  label: string
  value: number | string
  icon: React.ComponentType<{ className?: string }>
  iconColor: string
  bg: string
}) {
  return (
    <div className={`${bg} rounded-xl p-4 border border-gray-100`}>
      <div className="flex items-center justify-between">
        <p className="text-xs font-medium text-gray-500">{label}</p>
        <Icon className={`w-4 h-4 ${iconColor}`} />
      </div>
      <p className="text-xl font-bold text-gray-900 mt-2 tabular-nums">{value}</p>
    </div>
  )
}
