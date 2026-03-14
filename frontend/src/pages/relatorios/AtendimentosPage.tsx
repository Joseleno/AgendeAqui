import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { AttendanceChart } from '../../components/charts/AttendanceChart'
import { useAttendanceReport } from '../../hooks/useReports'
import { useProfessionals } from '../../hooks/useProfessionals'

export function AtendimentosPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [professionalId, setProfessionalId] = useState<string>('')

  const { data: profData } = useProfessionals(1, 100)
  const { data, isLoading } = useAttendanceReport(dateFrom, dateTo, professionalId || undefined)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Relatório de Atendimentos</h2>
        <div className="flex flex-wrap items-center gap-2">
          <select
            value={professionalId}
            onChange={(e) => setProfessionalId(e.target.value)}
            className="rounded-xl border border-gray-200 bg-gray-50/50 px-3 py-2 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
          >
            <option value="">Todos profissionais</option>
            {(profData?.items ?? []).map((p) => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>
          <DateRangePicker
            dateFrom={dateFrom}
            dateTo={dateTo}
            onDateFromChange={setDateFrom}
            onDateToChange={setDateTo}
          />
        </div>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : data ? (
        <>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <MetricCard label="Total" value={data.totalAppointments} color="text-brand-600" bg="bg-brand-50" />
            <MetricCard label="Concluídos" value={data.totalCompleted} color="text-green-600" bg="bg-green-50" />
            <MetricCard label="Cancelados" value={data.totalCancelled} color="text-red-600" bg="bg-red-50" />
            <MetricCard label="Faltaram" value={data.totalNoShow} color="text-gray-600" bg="bg-gray-100" />
          </div>

          {data.breakdown.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-200 p-4">
              <h3 className="text-sm font-semibold text-gray-700 mb-3">Por profissional</h3>
              <AttendanceChart data={data.breakdown} />
            </div>
          )}
        </>
      ) : (
        <p className="text-sm text-gray-400">Selecione um período</p>
      )}
    </div>
  )
}

function MetricCard({ label, value, color, bg }: { label: string; value: number; color: string; bg: string }) {
  return (
    <div className={`${bg} rounded-xl p-4 border border-gray-100`}>
      <p className="text-sm text-gray-600">{label}</p>
      <p className={`text-2xl font-bold ${color} mt-1`}>{value}</p>
    </div>
  )
}
