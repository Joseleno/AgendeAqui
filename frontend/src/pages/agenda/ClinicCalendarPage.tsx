import { useState } from 'react'
import { format } from 'date-fns'
import { CalendarDays, Clock } from 'lucide-react'
import { useClinicCalendar } from '../../hooks/useDashboard'
import { getStatusStyle } from '../../lib/status-colors'

export function ClinicCalendarPage() {
  const [date, setDate] = useState(format(new Date(), 'yyyy-MM-dd'))
  const { data, isLoading } = useClinicCalendar(date)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Calendário da Clínica</h2>
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
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-32 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : !data || data.professionals.length === 0 ? (
        <div className="text-center py-12">
          <CalendarDays className="w-10 h-10 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum agendamento nesta data</p>
        </div>
      ) : (
        <div className="space-y-4">
          {data.professionals.map((prof) => (
            <div key={prof.professionalId} className="bg-white rounded-xl border border-gray-200 overflow-hidden">
              <div className="bg-brand-50 px-4 py-3 border-b border-gray-100">
                <h3 className="font-semibold text-brand-800 text-sm">{prof.professionalName}</h3>
                <p className="text-xs text-brand-600">{prof.appointments.length} agendamento(s)</p>
              </div>
              <div className="divide-y divide-gray-100">
                {prof.appointments.map((apt) => {
                  const style = getStatusStyle(apt.status)
                  return (
                    <div key={apt.id} className="flex items-center justify-between px-4 py-3 hover:bg-gray-50 transition-colors">
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="flex items-center gap-1.5 text-gray-500 shrink-0">
                          <Clock className="w-3.5 h-3.5" />
                          <span className="text-xs font-medium tabular-nums">{apt.startTime} - {apt.endTime}</span>
                        </div>
                        <div className="min-w-0">
                          <p className="text-sm font-medium text-gray-800 truncate">{apt.clientName}</p>
                          <p className="text-xs text-gray-500 truncate">{apt.serviceName}</p>
                        </div>
                      </div>
                      <span className={`${style.bg} ${style.text} text-xs px-2.5 py-1 rounded-full shrink-0 ml-2 font-medium`}>
                        {style.label}
                      </span>
                    </div>
                  )
                })}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
