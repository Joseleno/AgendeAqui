import { useState } from 'react'
import type { Appointment } from '../../hooks/useAppointments'
import { useCancelAppointment } from '../../hooks/useAppointments'
import { getStatusStyle } from '../../lib/status-colors'

interface AppointmentDetailProps {
  appointment: Appointment
  onClose: () => void
}

export function AppointmentDetail({ appointment, onClose }: AppointmentDetailProps) {
  const [showCancel, setShowCancel] = useState(false)
  const [reason, setReason] = useState('')
  const cancelMutation = useCancelAppointment()
  const style = getStatusStyle(appointment.status)

  function handleCancel() {
    if (!reason.trim()) return
    cancelMutation.mutate(
      { id: appointment.id, reason },
      { onSuccess: onClose },
    )
  }

  const canCancel = appointment.status === 'Scheduled' || appointment.status === 'Confirmed'

  return (
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-md md:rounded-xl rounded-t-xl shadow-xl p-5 max-h-[80vh] overflow-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-800">Detalhes</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <div className="space-y-3 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-500">Status</span>
            <span className={`${style.bg} ${style.text} px-2 py-0.5 rounded-full text-xs`}>
              {style.label}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Cliente</span>
            <span className="text-gray-800 font-medium">{appointment.clientName}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Profissional</span>
            <span className="text-gray-800">{appointment.professionalName}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Servico</span>
            <span className="text-gray-800">{appointment.serviceName}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Data</span>
            <span className="text-gray-800">{appointment.date}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">Horario</span>
            <span className="text-gray-800">
              {appointment.startTime.slice(0, 5)} - {appointment.endTime.slice(0, 5)}
            </span>
          </div>
          {appointment.notes && (
            <div>
              <span className="text-gray-500">Observacoes</span>
              <p className="text-gray-800 mt-0.5">{appointment.notes}</p>
            </div>
          )}
        </div>

        {canCancel && !showCancel && (
          <button
            onClick={() => setShowCancel(true)}
            className="mt-4 w-full rounded-lg border border-red-300 text-red-600 py-2 text-sm font-medium hover:bg-red-50 transition-colors"
          >
            Cancelar agendamento
          </button>
        )}

        {showCancel && (
          <div className="mt-4 space-y-2">
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Motivo do cancelamento..."
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-red-500 focus:ring-1 focus:ring-red-500 outline-none"
              rows={2}
            />
            <div className="flex gap-2">
              <button
                onClick={() => setShowCancel(false)}
                className="flex-1 rounded-lg border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50"
              >
                Voltar
              </button>
              <button
                onClick={handleCancel}
                disabled={cancelMutation.isPending || !reason.trim()}
                className="flex-1 rounded-lg bg-red-600 py-2 text-sm text-white font-medium hover:bg-red-700 disabled:opacity-50"
              >
                {cancelMutation.isPending ? 'Cancelando...' : 'Confirmar'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
