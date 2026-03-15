import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { CalendarDays, FileText } from 'lucide-react'
import { api } from '../../../lib/api-client'
import type { Client } from '../../../hooks/useClients'
import type { Appointment, PagedResponse } from '../../../hooks/useAppointments'
import { getStatusStyle } from '../../../lib/status-colors'
import { NotesTab } from '../../../components/clinical/NotesTab'
import { EmptyState } from '../../../components/ui/EmptyState'
import { SkeletonText } from '../../../components/ui/Skeleton'
import { formatDate, formatTime, formatPhone } from '../../../lib/format'

type Tab = 'appointments' | 'notes'

export function ClienteDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [tab, setTab] = useState<Tab>('appointments')

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

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <SkeletonText lines={3} />
        </div>
      </div>
    )
  }

  if (!client) return <p className="text-sm text-gray-400">Cliente não encontrado</p>

  const appointments = appointmentsData?.items ?? []

  const tabs: { key: Tab; label: string; icon: typeof CalendarDays }[] = [
    { key: 'appointments', label: 'Agendamentos', icon: CalendarDays },
    { key: 'notes', label: 'Prontuário', icon: FileText },
  ]

  return (
    <div className="space-y-6 page-enter">
      <button onClick={() => navigate(-1)} className="text-sm text-brand-600 hover:text-brand-800">
        &larr; Voltar
      </button>

      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h1 className="text-xl font-bold text-gray-800">{client.name}</h1>
        <div className="mt-3 space-y-1 text-sm text-gray-600">
          <p>Email: {client.email}</p>
          <p>Telefone: {formatPhone(client.phone)}</p>
          <p>Cadastrado em: {formatDate(client.createdAt)}</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b border-gray-200 pb-px">
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            className={`flex items-center gap-1.5 px-3 py-2 text-sm font-medium rounded-t-lg transition-colors ${
              tab === t.key
                ? 'bg-white border border-b-white border-gray-200 text-brand-600 -mb-px'
                : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            <t.icon className="w-4 h-4" />
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'appointments' && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-3">Histórico de agendamentos</h2>
          {appointments.length === 0 ? (
            <EmptyState
              icon={CalendarDays}
              title="Nenhum agendamento"
              subtitle="Este cliente ainda não possui agendamentos"
            />
          ) : (
            <ul className="space-y-2">
              {appointments.map((a) => {
                const style = getStatusStyle(a.status)
                return (
                  <li key={a.id} className="flex items-center justify-between text-sm border-b border-gray-100 pb-2 last:border-b-0">
                    <div>
                      <p className="font-medium text-gray-800">{formatDate(a.date)} {formatTime(a.startTime)}</p>
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
      )}

      {tab === 'notes' && id && (
        <NotesTab clientId={id} />
      )}
    </div>
  )
}
