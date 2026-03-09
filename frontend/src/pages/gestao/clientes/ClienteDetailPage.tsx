import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { api } from '../../../lib/api-client'
import type { Client } from '../../../hooks/useClients'
import type { Appointment, PagedResponse } from '../../../hooks/useAppointments'
import { getStatusStyle } from '../../../lib/status-colors'

export function ClienteDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const { data: client, isLoading } = useQuery({
    queryKey: ['clients', id],
    queryFn: () => api.get<Client>(`/clients/${id}`),
    enabled: !!id,
  })

  const { data: appointmentsData } = useQuery({
    queryKey: ['appointments', 'client', id],
    queryFn: () => api.get<PagedResponse<Appointment>>(`/appointments?clientId=${id}&pageSize=20`),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-sm text-gray-400">Carregando...</p>
  if (!client) return <p className="text-sm text-gray-400">Cliente nao encontrado</p>

  const appointments = appointmentsData?.items ?? []

  return (
    <div className="space-y-6">
      <button onClick={() => navigate(-1)} className="text-sm text-indigo-600 hover:text-indigo-800">
        &larr; Voltar
      </button>

      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h1 className="text-xl font-bold text-gray-800">{client.name}</h1>
        <div className="mt-3 space-y-1 text-sm text-gray-600">
          <p>Email: {client.email}</p>
          <p>Telefone: {client.phone}</p>
          <p>Cadastrado em: {new Date(client.createdAt).toLocaleDateString('pt-BR')}</p>
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-sm font-semibold text-gray-700 mb-3">Historico de agendamentos</h2>
        {appointments.length === 0 ? (
          <p className="text-sm text-gray-400">Nenhum agendamento</p>
        ) : (
          <ul className="space-y-2">
            {appointments.map((a) => {
              const style = getStatusStyle(a.status)
              return (
                <li key={a.id} className="flex items-center justify-between text-sm border-b border-gray-100 pb-2 last:border-b-0">
                  <div>
                    <p className="font-medium text-gray-800">{a.date} {a.startTime.slice(0, 5)}</p>
                    <p className="text-xs text-gray-500">{a.serviceName} &middot; {a.professionalName}</p>
                  </div>
                  <span className={`${style.bg} ${style.text} text-xs px-2 py-0.5 rounded-full`}>
                    {style.label}
                  </span>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
