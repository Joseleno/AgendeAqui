import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '../../../lib/api-client'
import { useSchedules, useCreateSchedule, useDeactivateSchedule } from '../../../hooks/useSchedules'
import { PagedList } from '../../../components/ui/PagedList'
import { FormModal } from '../../../components/ui/FormModal'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'

const DAY_NAMES = ['Domingo', 'Segunda', 'Terca', 'Quarta', 'Quinta', 'Sexta', 'Sabado']

interface Professional {
  id: string
  name: string
}

interface PagedProfessionals {
  items: Professional[]
}

export function HorariosPage() {
  const [page, setPage] = useState(1)
  const [filterProfId, setFilterProfId] = useState<string>('')
  const [showCreate, setShowCreate] = useState(false)
  const [deactivateId, setDeactivateId] = useState<string | null>(null)

  const { data, isLoading } = useSchedules(page, filterProfId || undefined)
  const deactivateMutation = useDeactivateSchedule()

  const { data: profsData } = useQuery({
    queryKey: ['professionals', 'all'],
    queryFn: () => api.get<PagedProfessionals>('/professionals?pageSize=100'),
  })

  const professionals = profsData?.items ?? []
  const items = data?.items ?? []

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-800">Horarios</h1>
        <button onClick={() => setShowCreate(true)} className="rounded-lg bg-brand-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-brand-700">
          + Novo
        </button>
      </div>

      <select
        value={filterProfId}
        onChange={(e) => { setFilterProfId(e.target.value); setPage(1) }}
        className="rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500 max-w-xs"
      >
        <option value="">Todos profissionais</option>
        {professionals.map((p) => (
          <option key={p.id} value={p.id}>{p.name}</option>
        ))}
      </select>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum horario</p>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Profissional</th>
                <th className="px-4 py-3">Dia</th>
                <th className="px-4 py-3">Horario</th>
                <th className="px-4 py-3">Slot</th>
                <th className="px-4 py-3 w-16">Status</th>
                <th className="px-4 py-3 w-24"></th>
              </tr>
            </thead>
            <tbody>
              {items.map((s) => (
                <tr key={s.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{s.professionalName}</td>
                  <td className="px-4 py-3 text-gray-600">{DAY_NAMES[s.dayOfWeek] ?? s.dayOfWeek}</td>
                  <td className="px-4 py-3 text-gray-600">{s.startTime.slice(0, 5)} - {s.endTime.slice(0, 5)}</td>
                  <td className="px-4 py-3 text-gray-600">{s.slotDurationMinutes} min</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${s.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                      {s.isActive ? 'Ativo' : 'Inativo'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    {s.isActive && (
                      <button onClick={() => setDeactivateId(s.id)} className="text-xs text-red-600 hover:text-red-800">
                        Desativar
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <PagedList page={page} totalPages={data?.totalPages ?? 1} onPageChange={setPage} />

      {showCreate && (
        <ScheduleCreateModal
          professionals={professionals}
          onClose={() => setShowCreate(false)}
        />
      )}

      {deactivateId && (
        <ConfirmDialog
          title="Desativar horario"
          message="Tem certeza que deseja desativar este horario?"
          confirmLabel="Desativar"
          isPending={deactivateMutation.isPending}
          onConfirm={() => deactivateMutation.mutate(deactivateId, { onSuccess: () => setDeactivateId(null) })}
          onCancel={() => setDeactivateId(null)}
        />
      )}
    </div>
  )
}

function ScheduleCreateModal({
  professionals,
  onClose,
}: {
  professionals: Professional[]
  onClose: () => void
}) {
  const [professionalId, setProfessionalId] = useState('')
  const [dayOfWeek, setDayOfWeek] = useState('1')
  const [startTime, setStartTime] = useState('08:00')
  const [endTime, setEndTime] = useState('18:00')
  const [slotDuration, setSlotDuration] = useState('30')

  const createMutation = useCreateSchedule()

  return (
    <FormModal
      title="Novo horario"
      onClose={onClose}
      isPending={createMutation.isPending}
      onSubmit={(e) => {
        e.preventDefault()
        createMutation.mutate(
          { professionalId, dayOfWeek: Number(dayOfWeek), startTime, endTime, slotDurationMinutes: Number(slotDuration) },
          { onSuccess: onClose },
        )
      }}
    >
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Profissional</label>
        <select required value={professionalId} onChange={(e) => setProfessionalId(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500">
          <option value="">Selecione...</option>
          {professionals.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Dia da semana</label>
        <select value={dayOfWeek} onChange={(e) => setDayOfWeek(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500">
          {DAY_NAMES.map((name, i) => <option key={i} value={i}>{name}</option>)}
        </select>
      </div>
      <div className="grid grid-cols-2 gap-2">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Inicio</label>
          <input type="time" required value={startTime} onChange={(e) => setStartTime(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Fim</label>
          <input type="time" required value={endTime} onChange={(e) => setEndTime(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
        </div>
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Duracao do slot (min)</label>
        <input type="number" required min={5} value={slotDuration} onChange={(e) => setSlotDuration(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
      </div>
    </FormModal>
  )
}
