import { useState } from 'react'
import { format, startOfWeek, endOfWeek } from 'date-fns'
import { Calendar, Clock, Ban } from 'lucide-react'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { useMyCalendar, useMyFilledSlots } from '../../hooks/useMyCalendar'
import { getStatusStyle } from '../../lib/status-colors'

export function MeuCalendarioPage() {
  const now = new Date()
  const [dateFrom, setDateFrom] = useState(format(startOfWeek(now, { weekStartsOn: 1 }), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(endOfWeek(now, { weekStartsOn: 1 }), 'yyyy-MM-dd'))
  const [selectedDate, setSelectedDate] = useState(format(now, 'yyyy-MM-dd'))

  const { data: calendar, isLoading } = useMyCalendar(dateFrom, dateTo)
  const { data: filledSlots } = useMyFilledSlots(selectedDate)

  const fillPercent = filledSlots && filledSlots.totalSlots > 0
    ? Math.round((filledSlots.filledSlots / filledSlots.totalSlots) * 100)
    : 0

  return (
    <div className="space-y-4 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Meu Calendário</h1>
        <DateRangePicker dateFrom={dateFrom} dateTo={dateTo} onDateFromChange={setDateFrom} onDateToChange={setDateTo} />
      </div>

      {filledSlots && (
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-gray-700">Slots preenchidos ({selectedDate})</span>
            <span className="text-sm font-semibold text-brand-600">{filledSlots.filledSlots}/{filledSlots.totalSlots} ({fillPercent}%)</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2.5">
            <div className="bg-brand-500 h-2.5 rounded-full transition-all duration-500" style={{ width: `${fillPercent}%` }} />
          </div>
        </div>
      )}

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-24 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : !calendar || calendar.days.length === 0 ? (
        <div className="text-center py-12">
          <Calendar className="w-10 h-10 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum agendamento neste período</p>
        </div>
      ) : (
        <div className="space-y-4">
          {calendar.days.map((day) => (
            <div
              key={day.date}
              className={`bg-white rounded-xl border border-gray-200 overflow-hidden ${day.date === selectedDate ? 'ring-2 ring-brand-500' : ''}`}
              onClick={() => setSelectedDate(day.date)}
            >
              <div className="bg-gray-50 px-4 py-2.5 border-b border-gray-100 flex items-center justify-between cursor-pointer">
                <span className="font-semibold text-gray-800 text-sm">{day.date}</span>
                <span className="text-xs text-gray-500">{day.appointments.length} agendamento(s)</span>
              </div>

              {day.absences.length > 0 && (
                <div className="px-4 py-2 bg-amber-50 border-b border-amber-100">
                  {day.absences.map((abs) => (
                    <div key={abs.id} className="flex items-center gap-2 text-xs text-amber-700">
                      <Ban className="w-3 h-3" />
                      <span>{abs.isFullDay ? 'Dia inteiro bloqueado' : `Bloqueio: ${abs.startTime} - ${abs.endTime}`}</span>
                      {abs.reason && <span className="text-amber-500">({abs.reason})</span>}
                    </div>
                  ))}
                </div>
              )}

              <div className="divide-y divide-gray-100">
                {day.appointments.map((apt) => {
                  const style = getStatusStyle(apt.status)
                  return (
                    <div key={apt.id} className="flex items-center justify-between px-4 py-2.5 hover:bg-gray-50 transition-colors">
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="flex items-center gap-1 text-gray-500 shrink-0">
                          <Clock className="w-3.5 h-3.5" />
                          <span className="text-xs font-medium tabular-nums">{apt.startTime} - {apt.endTime}</span>
                        </div>
                        <div className="min-w-0">
                          <p className="text-sm font-medium text-gray-800 truncate">{apt.clientName}</p>
                          <p className="text-xs text-gray-500 truncate">{apt.serviceName}</p>
                        </div>
                      </div>
                      <span className={`${style.bg} ${style.text} text-xs px-2 py-0.5 rounded-full shrink-0 ml-2 font-medium`}>
                        {style.label}
                      </span>
                    </div>
                  )
                })}
                {day.appointments.length === 0 && (
                  <p className="px-4 py-3 text-xs text-gray-400">Sem agendamentos</p>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
