import { useState } from 'react'
import { format } from 'date-fns'
import { CalendarDays, CheckCircle, AlertTriangle } from 'lucide-react'
import { useScheduleConflicts } from '../../hooks/useDashboard'

export function ConflitosPage() {
  const [date, setDate] = useState(format(new Date(), 'yyyy-MM-dd'))
  const { data, isLoading } = useScheduleConflicts(date)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Conflitos de Agenda</h2>
        <div className="relative">
          <CalendarDays className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          <input
            type="date"
            value={date}
            onChange={(e) => setDate(e.target.value)}
            aria-label="Data"
            className="rounded-xl border border-gray-200 bg-gray-50/50 pl-8 pr-3 py-2 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
          />
        </div>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : !data || data.length === 0 ? (
        <div className="text-center py-12">
          <CheckCircle className="w-10 h-10 text-green-400 mx-auto mb-2" />
          <p className="text-sm text-gray-500 font-medium">Nenhum conflito detectado</p>
          <p className="text-xs text-gray-400 mt-1">Todas as agendas estão livres de sobreposições</p>
        </div>
      ) : (
        <div className="space-y-3">
          {data.map((c, i) => (
            <div key={i} className="bg-red-50 rounded-xl border border-red-200 p-4">
              <div className="flex items-center gap-2 mb-2">
                <AlertTriangle className="w-4 h-4 text-red-500" />
                <span className="font-semibold text-red-800 text-sm">{c.professionalName}</span>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm">
                <div className="bg-white rounded-lg p-3 border border-red-100">
                  <p className="text-xs text-gray-500 mb-1">Agendamento 1</p>
                  <p className="font-medium text-gray-800">{c.client1}</p>
                  <p className="text-gray-500">{c.startTime1} — {c.endTime1}</p>
                </div>
                <div className="bg-white rounded-lg p-3 border border-red-100">
                  <p className="text-xs text-gray-500 mb-1">Agendamento 2</p>
                  <p className="font-medium text-gray-800">{c.client2}</p>
                  <p className="text-gray-500">{c.startTime2} — {c.endTime2}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
