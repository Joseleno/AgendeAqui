import { useState, useMemo } from 'react'
import { useSearchParams, useNavigate } from 'react-router-dom'
import { CalendarDays, Clock, Check } from 'lucide-react'
import { format } from 'date-fns'
import { useProfessionals } from '../../hooks/useProfessionals'
import { useServices } from '../../hooks/useServices'
import { useAvailability, useCreateAppointment } from '../../hooks/useAppointments'
import { useAuthState } from '../../hooks/useAuth'

export function AgendarPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { clientId } = useAuthState()
  const preselectedProfId = searchParams.get('professionalId') ?? ''

  const [step, setStep] = useState(1)
  const [professionalId, setProfessionalId] = useState(preselectedProfId)
  const [serviceId, setServiceId] = useState('')
  const [date, setDate] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [selectedSlot, setSelectedSlot] = useState<{ start: string; end: string } | null>(null)
  const [notes, setNotes] = useState('')

  const { data: profData } = useProfessionals(1, 100)
  const { data: svcData } = useServices(1, 100)
  const { data: availability, isLoading: loadingSlots } = useAvailability(professionalId, date, serviceId)
  const createAppointment = useCreateAppointment()

  const professionals = profData?.items ?? []
  const services = svcData?.items ?? []

  const selectedProfName = useMemo(() => professionals.find((p) => p.id === professionalId)?.name ?? '', [professionals, professionalId])
  const selectedSvcName = useMemo(() => services.find((s) => s.id === serviceId)?.name ?? '', [services, serviceId])

  function handleConfirm() {
    if (!selectedSlot) return
    createAppointment.mutate(
      { professionalId, serviceId, clientId: clientId ?? '', date, startTime: selectedSlot.start, notes: notes || undefined },
      { onSuccess: () => navigate('/meus-agendamentos') },
    )
  }

  return (
    <div className="max-w-lg mx-auto space-y-6 animate-fade-in">
      <h1 className="text-xl font-bold text-gray-800">Agendar Consulta</h1>

      {/* Step indicator */}
      <div className="flex items-center gap-2">
        {[1, 2, 3].map((s) => (
          <div key={s} className="flex items-center gap-2">
            <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${
              step >= s ? 'bg-brand-500 text-white' : 'bg-gray-200 text-gray-500'
            }`}>
              {step > s ? <Check className="w-4 h-4" /> : s}
            </div>
            {s < 3 && <div className={`w-8 h-0.5 ${step > s ? 'bg-brand-500' : 'bg-gray-200'}`} />}
          </div>
        ))}
      </div>

      {step === 1 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="font-semibold text-gray-800">Selecione profissional e serviço</h2>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Profissional</label>
            <select
              value={professionalId}
              onChange={(e) => setProfessionalId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
            >
              <option value="">Selecione...</option>
              {professionals.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Serviço</label>
            <select
              value={serviceId}
              onChange={(e) => setServiceId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
            >
              <option value="">Selecione...</option>
              {services.map((s) => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
          </div>
          <button
            onClick={() => setStep(2)}
            disabled={!professionalId || !serviceId}
            className="w-full rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all"
          >
            Próximo
          </button>
        </div>
      )}

      {step === 2 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="font-semibold text-gray-800">Selecione data e horário</h2>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Data</label>
            <div className="relative">
              <CalendarDays className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="date"
                value={date}
                min={format(new Date(), 'yyyy-MM-dd')}
                onChange={(e) => { setDate(e.target.value); setSelectedSlot(null) }}
                className="w-full rounded-lg border border-gray-300 pl-9 pr-3 py-2 text-sm outline-none focus:border-brand-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Horários disponíveis</label>
            {loadingSlots ? (
              <div className="grid grid-cols-3 gap-2">
                {[1, 2, 3, 4, 5, 6].map((i) => (
                  <div key={i} className="h-10 bg-gray-100 rounded-lg animate-pulse" />
                ))}
              </div>
            ) : !availability || availability.slots.length === 0 ? (
              <p className="text-sm text-gray-400">Nenhum horário disponível nesta data</p>
            ) : (
              <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
                {availability.slots.map((slot) => (
                  <button
                    key={slot.start}
                    onClick={() => setSelectedSlot(slot)}
                    className={`flex items-center justify-center gap-1 px-3 py-2 rounded-lg border text-sm font-medium transition-all ${
                      selectedSlot?.start === slot.start
                        ? 'bg-brand-500 text-white border-brand-500'
                        : 'bg-white text-gray-700 border-gray-200 hover:border-brand-300 hover:bg-brand-50'
                    }`}
                  >
                    <Clock className="w-3.5 h-3.5" />
                    {slot.start.slice(0, 5)}
                  </button>
                ))}
              </div>
            )}
          </div>

          <div className="flex gap-3">
            <button onClick={() => setStep(1)} className="flex-1 rounded-xl border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-all">
              Voltar
            </button>
            <button
              onClick={() => setStep(3)}
              disabled={!selectedSlot}
              className="flex-1 rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all"
            >
              Próximo
            </button>
          </div>
        </div>
      )}

      {step === 3 && selectedSlot && (
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="font-semibold text-gray-800">Confirmar agendamento</h2>
          <div className="bg-brand-50 rounded-lg p-4 space-y-2 text-sm">
            <p><span className="font-medium text-gray-700">Profissional:</span> {selectedProfName}</p>
            <p><span className="font-medium text-gray-700">Serviço:</span> {selectedSvcName}</p>
            <p><span className="font-medium text-gray-700">Data:</span> {date}</p>
            <p><span className="font-medium text-gray-700">Horário:</span> {selectedSlot.start.slice(0, 5)} - {selectedSlot.end.slice(0, 5)}</p>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Observações (opcional)</label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 resize-none"
              placeholder="Alguma observação para o profissional..."
            />
          </div>
          <div className="flex gap-3">
            <button onClick={() => setStep(2)} className="flex-1 rounded-xl border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-all">
              Voltar
            </button>
            <button
              onClick={handleConfirm}
              disabled={createAppointment.isPending}
              className="flex-1 rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all"
            >
              {createAppointment.isPending ? 'Agendando...' : 'Confirmar'}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
