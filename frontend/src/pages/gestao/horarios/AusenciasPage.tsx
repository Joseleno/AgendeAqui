import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { CalendarOff, Trash2 } from 'lucide-react'
import { DateRangePicker } from '../../../components/ui/DateRangePicker'
import { FormModal } from '../../../components/ui/FormModal'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { useAbsences, useCreateAbsence, useDeleteAbsence } from '../../../hooks/useAbsences'
import { useProfessionals } from '../../../hooks/useProfessionals'

export function AusenciasPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 7), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000), 'yyyy-MM-dd'))
  const [filterProfId, setFilterProfId] = useState('')
  const [showCreate, setShowCreate] = useState(false)
  const [deleteId, setDeleteId] = useState<string | null>(null)

  const { data: profData } = useProfessionals(1, 100)
  const { data: absences, isLoading } = useAbsences(filterProfId || undefined, dateFrom, dateTo)
  const createMutation = useCreateAbsence()
  const deleteMutation = useDeleteAbsence()

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Ausências e Bloqueios</h2>
        <button onClick={() => setShowCreate(true)} className="rounded-lg bg-brand-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-brand-700">
          + Nova Ausência
        </button>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <DateRangePicker dateFrom={dateFrom} dateTo={dateTo} onDateFromChange={setDateFrom} onDateToChange={setDateTo} />
        <select
          value={filterProfId}
          onChange={(e) => setFilterProfId(e.target.value)}
          className="rounded-xl border border-gray-200 bg-gray-50/50 px-3 py-2 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
        >
          <option value="">Todos profissionais</option>
          {(profData?.items ?? []).map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : !absences || absences.length === 0 ? (
        <div className="text-center py-12">
          <CalendarOff className="w-10 h-10 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhuma ausência encontrada</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Data</th>
                <th className="px-4 py-3 hidden sm:table-cell">Horário</th>
                <th className="px-4 py-3 hidden sm:table-cell">Motivo</th>
                <th className="px-4 py-3 w-16"></th>
              </tr>
            </thead>
            <tbody>
              {absences.map((a) => (
                <tr key={a.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{a.date}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">
                    {a.isFullDay ? 'Dia inteiro' : `${a.startTime} - ${a.endTime}`}
                  </td>
                  <td className="px-4 py-3 text-gray-500 hidden sm:table-cell">{a.reason ?? '-'}</td>
                  <td className="px-4 py-3">
                    <button onClick={() => setDeleteId(a.id)} className="text-red-500 hover:text-red-700" title="Excluir">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showCreate && (
        <AbsenceFormModal
          professionals={profData?.items ?? []}
          isPending={createMutation.isPending}
          onClose={() => setShowCreate(false)}
          onSubmit={(payload) => createMutation.mutate(payload, { onSuccess: () => setShowCreate(false) })}
        />
      )}

      {deleteId && (
        <ConfirmDialog
          title="Excluir ausência"
          message="Tem certeza que deseja excluir esta ausência?"
          confirmLabel="Excluir"
          isPending={deleteMutation.isPending}
          onConfirm={() => deleteMutation.mutate(deleteId, { onSuccess: () => setDeleteId(null) })}
          onCancel={() => setDeleteId(null)}
        />
      )}
    </div>
  )
}

function AbsenceFormModal({
  professionals,
  isPending,
  onClose,
  onSubmit,
}: {
  professionals: { id: string; name: string }[]
  isPending: boolean
  onClose: () => void
  onSubmit: (payload: { professionalId: string; date: string; startTime?: string; endTime?: string; reason?: string }) => void
}) {
  const [professionalId, setProfessionalId] = useState('')
  const [date, setDate] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [isFullDay, setIsFullDay] = useState(true)
  const [startTime, setStartTime] = useState('')
  const [endTime, setEndTime] = useState('')
  const [reason, setReason] = useState('')

  return (
    <FormModal
      title="Nova Ausência"
      onClose={onClose}
      isPending={isPending}
      onSubmit={(e) => {
        e.preventDefault()
        onSubmit({
          professionalId,
          date,
          startTime: isFullDay ? undefined : startTime,
          endTime: isFullDay ? undefined : endTime,
          reason: reason || undefined,
        })
      }}
    >
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Profissional</label>
        <select required value={professionalId} onChange={(e) => setProfessionalId(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500">
          <option value="">Selecione...</option>
          {professionals.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Data</label>
        <input type="date" required value={date} onChange={(e) => setDate(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
      </div>
      <div className="flex items-center gap-2">
        <input type="checkbox" id="fullDay" checked={isFullDay} onChange={(e) => setIsFullDay(e.target.checked)} className="rounded" />
        <label htmlFor="fullDay" className="text-sm text-gray-700">Dia inteiro</label>
      </div>
      {!isFullDay && (
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Início</label>
            <input type="time" required value={startTime} onChange={(e) => setStartTime(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Fim</label>
            <input type="time" required value={endTime} onChange={(e) => setEndTime(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
          </div>
        </div>
      )}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Motivo (opcional)</label>
        <input value={reason} onChange={(e) => setReason(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" placeholder="Ex: Férias, consulta médica..." />
      </div>
    </FormModal>
  )
}
