import { useState } from 'react'
import { useServices, useCreateService, useUpdateService } from '../../../hooks/useServices'
import type { Service } from '../../../hooks/useServices'
import { PagedList } from '../../../components/ui/PagedList'
import { FormModal } from '../../../components/ui/FormModal'

export function ServicosPage() {
  const [page, setPage] = useState(1)
  const [editItem, setEditItem] = useState<Service | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  const { data, isLoading } = useServices(page)
  const createMutation = useCreateService()
  const updateMutation = useUpdateService()

  const items = data?.items ?? []

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-800">Servicos</h1>
        <button onClick={() => setShowCreate(true)} className="rounded-lg bg-indigo-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-indigo-700">
          + Novo
        </button>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum servico</p>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Nome</th>
                <th className="px-4 py-3">Duracao</th>
                <th className="px-4 py-3">Preco</th>
                <th className="px-4 py-3 w-20"></th>
              </tr>
            </thead>
            <tbody>
              {items.map((s) => (
                <tr key={s.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{s.name}</td>
                  <td className="px-4 py-3 text-gray-600">{s.durationMinutes} min</td>
                  <td className="px-4 py-3 text-gray-600">R$ {s.price.toFixed(2)}</td>
                  <td className="px-4 py-3">
                    <button onClick={() => setEditItem(s)} className="text-xs text-indigo-600 hover:text-indigo-800">
                      Editar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <PagedList page={page} totalPages={data?.totalPages ?? 1} onPageChange={setPage} />

      {showCreate && (
        <ServiceFormModal
          title="Novo servico"
          isPending={createMutation.isPending}
          onClose={() => setShowCreate(false)}
          onSubmit={(p) => createMutation.mutate(p, { onSuccess: () => setShowCreate(false) })}
        />
      )}

      {editItem && (
        <ServiceFormModal
          title="Editar servico"
          initial={editItem}
          isPending={updateMutation.isPending}
          onClose={() => setEditItem(null)}
          onSubmit={(p) => updateMutation.mutate({ id: editItem.id, ...p }, { onSuccess: () => setEditItem(null) })}
        />
      )}
    </div>
  )
}

function ServiceFormModal({
  title,
  initial,
  isPending,
  onClose,
  onSubmit,
}: {
  title: string
  initial?: { name: string; durationMinutes: number; price: number }
  isPending: boolean
  onClose: () => void
  onSubmit: (payload: { name: string; durationMinutes: number; price: number }) => void
}) {
  const [name, setName] = useState(initial?.name ?? '')
  const [duration, setDuration] = useState(String(initial?.durationMinutes ?? 30))
  const [price, setPrice] = useState(String(initial?.price ?? ''))

  return (
    <FormModal title={title} onClose={onClose} isPending={isPending} onSubmit={(e) => { e.preventDefault(); onSubmit({ name, durationMinutes: Number(duration), price: Number(price) }) }}>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
        <input required value={name} onChange={(e) => setName(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Duracao (min)</label>
        <input type="number" required min={5} value={duration} onChange={(e) => setDuration(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Preco (R$)</label>
        <input type="number" required min={0} step={0.01} value={price} onChange={(e) => setPrice(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500" />
      </div>
    </FormModal>
  )
}
