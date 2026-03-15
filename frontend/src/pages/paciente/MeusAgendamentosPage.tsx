import { useState } from 'react'
import { CalendarDays, Clock } from 'lucide-react'
import { useMyAppointments } from '../../hooks/usePatientPortal'
import { useCancelAppointment } from '../../hooks/useAppointments'
import { ConfirmDialog } from '../../components/ui/ConfirmDialog'
import { PagedList } from '../../components/ui/PagedList'
import { EmptyState } from '../../components/ui/EmptyState'
import { SkeletonCard } from '../../components/ui/Skeleton'
import { getStatusStyle } from '../../lib/status-colors'
import { formatDate, formatTime } from '../../lib/format'

const statusTabs = [
  { value: '', label: 'Todos' },
  { value: 'Scheduled', label: 'Agendados' },
  { value: 'Completed', label: 'Concluídos' },
  { value: 'Cancelled', label: 'Cancelados' },
]

export function MeusAgendamentosPage() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('')
  const [cancelId, setCancelId] = useState<string | null>(null)
  const [cancelReason, setCancelReason] = useState('')

  const { data, isLoading } = useMyAppointments(page, 10, status || undefined)
  const cancelMutation = useCancelAppointment()
  const items = data?.items ?? []

  function handleCancel() {
    if (!cancelId) return
    cancelMutation.mutate(
      { id: cancelId, reason: cancelReason || 'Cancelado pelo paciente' },
      { onSuccess: () => { setCancelId(null); setCancelReason('') } },
    )
  }

  return (
    <div className="space-y-4 animate-fade-in">
      <h1 className="text-xl font-bold text-gray-800">Meus Agendamentos</h1>

      <div className="flex gap-1 border-b border-gray-200 pb-px">
        {statusTabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => { setStatus(tab.value); setPage(1) }}
            className={`px-3 py-2 text-sm font-medium rounded-t-lg transition-colors ${
              status === tab.value
                ? 'bg-white border border-b-white border-gray-200 text-brand-600 -mb-px'
                : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <SkeletonCard key={i} className="h-24" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <EmptyState
          icon={CalendarDays}
          title="Nenhum agendamento encontrado"
          subtitle="Você ainda não possui agendamentos nesta categoria"
        />
      ) : (
        <div className="space-y-3">
          {items.map((apt) => {
            const style = getStatusStyle(apt.status)
            const canCancel = apt.status === 'Scheduled' || apt.status === 'Confirmed'
            return (
              <div key={apt.id} className="bg-white rounded-xl border border-gray-200 p-4 hover:shadow-card-hover transition-shadow">
                <div className="flex items-start justify-between">
                  <div className="min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <CalendarDays className="w-4 h-4 text-gray-400" />
                      <span className="text-sm font-medium text-gray-800">{formatDate(apt.date)}</span>
                      <Clock className="w-4 h-4 text-gray-400 ml-1" />
                      <span className="text-sm text-gray-600">{formatTime(apt.startTime)} - {formatTime(apt.endTime)}</span>
                    </div>
                    <p className="text-sm font-semibold text-gray-800">{apt.professionalName}</p>
                    <p className="text-xs text-gray-500">{apt.serviceName}</p>
                    {apt.notes && <p className="text-xs text-gray-400 mt-1">{apt.notes}</p>}
                  </div>
                  <div className="flex items-center gap-2 shrink-0 ml-3">
                    <span className={`${style.bg} ${style.text} text-xs px-2.5 py-1 rounded-full font-medium`}>
                      {style.label}
                    </span>
                    {canCancel && (
                      <button
                        onClick={() => setCancelId(apt.id)}
                        className="text-xs text-red-500 hover:text-red-700 font-medium"
                      >
                        Cancelar
                      </button>
                    )}
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}

      <PagedList page={page} totalPages={data?.totalPages ?? 1} onPageChange={setPage} />

      {cancelId && (
        <ConfirmDialog
          title="Cancelar agendamento"
          message={
            <div className="space-y-3">
              <p>Tem certeza que deseja cancelar este agendamento?</p>
              <textarea
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
                placeholder="Motivo do cancelamento..."
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 resize-none"
                rows={2}
              />
            </div>
          }
          confirmLabel="Cancelar agendamento"
          isPending={cancelMutation.isPending}
          onConfirm={handleCancel}
          onCancel={() => { setCancelId(null); setCancelReason('') }}
        />
      )}
    </div>
  )
}
