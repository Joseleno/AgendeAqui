import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { Download } from 'lucide-react'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { useExportCsv } from '../../hooks/useDashboard'
import { useProfessionals } from '../../hooks/useProfessionals'

export function ExportarPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [professionalId, setProfessionalId] = useState('')

  const { data: profData } = useProfessionals(1, 100)
  const exportCsv = useExportCsv()

  function handleExport() {
    exportCsv.mutate({
      from: dateFrom,
      to: dateTo,
      professionalId: professionalId || undefined,
    })
  }

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold text-gray-800">Exportar Agendamentos</h2>

      <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Período</label>
          <DateRangePicker dateFrom={dateFrom} dateTo={dateTo} onDateFromChange={setDateFrom} onDateToChange={setDateTo} />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Profissional (opcional)</label>
          <select
            value={professionalId}
            onChange={(e) => setProfessionalId(e.target.value)}
            className="rounded-xl border border-gray-200 bg-gray-50/50 px-3 py-2 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200 w-full sm:w-auto"
          >
            <option value="">Todos</option>
            {(profData?.items ?? []).map((p) => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>
        </div>

        <button
          onClick={handleExport}
          disabled={exportCsv.isPending}
          className="flex items-center gap-2 rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all duration-200 shadow-sm hover:shadow-md"
        >
          <Download className="w-4 h-4" />
          {exportCsv.isPending ? 'Exportando...' : 'Exportar CSV'}
        </button>
      </div>
    </div>
  )
}
