import { useState, useMemo } from 'react'
import {
  format,
  addDays,
  subDays,
  addWeeks,
  subWeeks,
  addMonths,
  subMonths,
  startOfWeek,
  endOfWeek,
  startOfMonth,
  endOfMonth,
} from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { useAppointments, useAppointmentRealtime } from '../../hooks/useAppointments'
import type { Appointment } from '../../hooks/useAppointments'
import { DayView } from '../../components/agenda/DayView'
import { WeekView } from '../../components/agenda/WeekView'
import { MonthView } from '../../components/agenda/MonthView'
import { AppointmentDetail } from '../../components/agenda/AppointmentDetail'
import { NewAppointmentModal } from '../../components/agenda/NewAppointmentModal'

type ViewMode = 'day' | 'week' | 'month'

export function AgendaPage() {
  const [viewMode, setViewMode] = useState<ViewMode>('day')
  const [currentDate, setCurrentDate] = useState(new Date())
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null)
  const [showNewModal, setShowNewModal] = useState(false)

  useAppointmentRealtime()

  const { dateFrom, dateTo } = useMemo(() => {
    if (viewMode === 'day') {
      const d = format(currentDate, 'yyyy-MM-dd')
      return { dateFrom: d, dateTo: d }
    }
    if (viewMode === 'week') {
      const ws = startOfWeek(currentDate, { weekStartsOn: 1 })
      const we = endOfWeek(currentDate, { weekStartsOn: 1 })
      return { dateFrom: format(ws, 'yyyy-MM-dd'), dateTo: format(we, 'yyyy-MM-dd') }
    }
    const ms = startOfMonth(currentDate)
    const me = endOfMonth(currentDate)
    return { dateFrom: format(ms, 'yyyy-MM-dd'), dateTo: format(me, 'yyyy-MM-dd') }
  }, [viewMode, currentDate])

  const { data, isLoading } = useAppointments(dateFrom, dateTo)
  const appointments = data?.items ?? []

  function navigate(direction: 1 | -1) {
    if (viewMode === 'day') {
      setCurrentDate((d) => (direction === 1 ? addDays(d, 1) : subDays(d, 1)))
    } else if (viewMode === 'week') {
      setCurrentDate((d) => (direction === 1 ? addWeeks(d, 1) : subWeeks(d, 1)))
    } else {
      setCurrentDate((d) => (direction === 1 ? addMonths(d, 1) : subMonths(d, 1)))
    }
  }

  function handleMonthDayClick(date: Date) {
    setCurrentDate(date)
    setViewMode('day')
  }

  const title = viewMode === 'day'
    ? format(currentDate, "EEEE, d 'de' MMMM", { locale: ptBR })
    : viewMode === 'week'
      ? `Semana de ${format(startOfWeek(currentDate, { weekStartsOn: 1 }), "d MMM", { locale: ptBR })}`
      : format(currentDate, "MMMM yyyy", { locale: ptBR })

  const views: { key: ViewMode; label: string }[] = [
    { key: 'day', label: 'Dia' },
    { key: 'week', label: 'Semana' },
    { key: 'month', label: 'Mes' },
  ]

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate(-1)}
            className="w-8 h-8 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 flex items-center justify-center"
          >
            &lt;
          </button>
          <h1 className="text-lg font-semibold text-gray-800 capitalize">{title}</h1>
          <button
            onClick={() => navigate(1)}
            className="w-8 h-8 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 flex items-center justify-center"
          >
            &gt;
          </button>
          <button
            onClick={() => setCurrentDate(new Date())}
            className="text-xs text-indigo-600 hover:text-indigo-800 font-medium ml-2"
          >
            Hoje
          </button>
        </div>

        <div className="flex items-center gap-2">
          <div className="flex rounded-lg border border-gray-300 overflow-hidden">
            {views.map((v) => (
              <button
                key={v.key}
                onClick={() => setViewMode(v.key)}
                className={`px-3 py-1.5 text-xs font-medium transition-colors ${
                  viewMode === v.key
                    ? 'bg-indigo-600 text-white'
                    : 'text-gray-600 hover:bg-gray-50'
                }`}
              >
                {v.label}
              </button>
            ))}
          </div>
          <button
            onClick={() => setShowNewModal(true)}
            className="rounded-lg bg-indigo-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-indigo-700 transition-colors"
          >
            + Novo
          </button>
        </div>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400 text-center py-8">Carregando...</p>
      ) : (
        <>
          {viewMode === 'day' && (
            <DayView appointments={appointments} onSelect={setSelectedAppointment} />
          )}
          {viewMode === 'week' && (
            <WeekView
              currentDate={currentDate}
              appointments={appointments}
              onSelect={setSelectedAppointment}
            />
          )}
          {viewMode === 'month' && (
            <MonthView
              currentDate={currentDate}
              appointments={appointments}
              onDayClick={handleMonthDayClick}
            />
          )}
        </>
      )}

      {selectedAppointment && (
        <AppointmentDetail
          appointment={selectedAppointment}
          onClose={() => setSelectedAppointment(null)}
        />
      )}

      {showNewModal && (
        <NewAppointmentModal
          initialDate={format(currentDate, 'yyyy-MM-dd')}
          onClose={() => setShowNewModal(false)}
        />
      )}
    </div>
  )
}
