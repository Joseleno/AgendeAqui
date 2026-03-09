import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '../../lib/api-client'
import { useCreateAppointment, useAvailability } from '../../hooks/useAppointments'

interface Professional {
  id: string
  name: string
}

interface Service {
  id: string
  name: string
  durationMinutes: number
  price: number
}

interface Client {
  id: string
  name: string
}

interface PagedResponse<T> {
  items: T[]
}

interface NewAppointmentModalProps {
  initialDate?: string
  onClose: () => void
}

export function NewAppointmentModal({ initialDate, onClose }: NewAppointmentModalProps) {
  const [professionalId, setProfessionalId] = useState('')
  const [serviceId, setServiceId] = useState('')
  const [clientId, setClientId] = useState('')
  const [date, setDate] = useState(initialDate ?? new Date().toISOString().slice(0, 10))
  const [startTime, setStartTime] = useState('')
  const [notes, setNotes] = useState('')

  const createMutation = useCreateAppointment()

  const { data: professionalsData } = useQuery({
    queryKey: ['professionals'],
    queryFn: () => api.get<PagedResponse<Professional>>('/professionals?pageSize=100'),
  })

  const { data: servicesData } = useQuery({
    queryKey: ['services'],
    queryFn: () => api.get<PagedResponse<Service>>('/services?pageSize=100'),
  })

  const { data: clientsData } = useQuery({
    queryKey: ['clients'],
    queryFn: () => api.get<PagedResponse<Client>>('/clients?pageSize=100'),
  })

  const { data: availability } = useAvailability(professionalId, date, serviceId)

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    createMutation.mutate(
      { professionalId, serviceId, clientId, date, startTime, notes: notes || undefined },
      { onSuccess: onClose },
    )
  }

  const professionals = professionalsData?.items ?? []
  const services = servicesData?.items ?? []
  const clients = clientsData?.items ?? []
  const slots = availability?.slots ?? []

  return (
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-md md:rounded-xl rounded-t-xl shadow-xl p-5 max-h-[80vh] overflow-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-800">Novo agendamento</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Profissional</label>
            <select
              required
              value={professionalId}
              onChange={(e) => { setProfessionalId(e.target.value); setStartTime('') }}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
            >
              <option value="">Selecione...</option>
              {professionals.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Servico</label>
            <select
              required
              value={serviceId}
              onChange={(e) => { setServiceId(e.target.value); setStartTime('') }}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
            >
              <option value="">Selecione...</option>
              {services.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} ({s.durationMinutes}min — R$ {s.price.toFixed(2)})
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Cliente</label>
            <select
              required
              value={clientId}
              onChange={(e) => setClientId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
            >
              <option value="">Selecione...</option>
              {clients.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Data</label>
            <input
              type="date"
              required
              value={date}
              onChange={(e) => { setDate(e.target.value); setStartTime('') }}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Horario</label>
            {professionalId && serviceId && date ? (
              slots.length > 0 ? (
                <div className="flex flex-wrap gap-1.5">
                  {slots.map((slot) => (
                    <button
                      key={slot.start}
                      type="button"
                      onClick={() => setStartTime(slot.start)}
                      className={`px-3 py-1.5 text-xs rounded-lg border transition-colors ${
                        startTime === slot.start
                          ? 'bg-indigo-600 text-white border-indigo-600'
                          : 'border-gray-300 text-gray-700 hover:border-indigo-400'
                      }`}
                    >
                      {slot.start.slice(0, 5)}
                    </button>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-gray-400">Nenhum horario disponivel</p>
              )
            ) : (
              <p className="text-sm text-gray-400">Selecione profissional, servico e data</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Observacoes</label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
              rows={2}
            />
          </div>

          <button
            type="submit"
            disabled={createMutation.isPending || !startTime}
            className="w-full rounded-lg bg-indigo-600 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            {createMutation.isPending ? 'Criando...' : 'Criar agendamento'}
          </button>
        </form>
      </div>
    </div>
  )
}
