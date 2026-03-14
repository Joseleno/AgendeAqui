import { useState } from 'react'
import { useProfessionals, useCreateProfessional, useUpdateProfessional } from '../../../hooks/useProfessionals'
import type { Professional } from '../../../hooks/useProfessionals'
import { PagedList } from '../../../components/ui/PagedList'
import { FormModal } from '../../../components/ui/FormModal'

export function ProfissionaisPage() {
  const [page, setPage] = useState(1)
  const [editItem, setEditItem] = useState<Professional | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  const { data, isLoading } = useProfessionals(page)
  const createMutation = useCreateProfessional()
  const updateMutation = useUpdateProfessional()

  const items = data?.items ?? []

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-800">Profissionais</h1>
        <button onClick={() => setShowCreate(true)} className="rounded-lg bg-brand-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-brand-700">
          + Novo
        </button>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum profissional</p>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Nome</th>
                <th className="px-4 py-3 hidden sm:table-cell">Email</th>
                <th className="px-4 py-3 hidden sm:table-cell">Telefone</th>
                <th className="px-4 py-3 w-16">Status</th>
                <th className="px-4 py-3 w-20"></th>
              </tr>
            </thead>
            <tbody>
              {items.map((p) => (
                <tr key={p.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{p.name}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{p.email}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{p.phone}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${p.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                      {p.isActive ? 'Ativo' : 'Inativo'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <button onClick={() => setEditItem(p)} className="text-xs text-brand-600 hover:text-brand-800">
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
        <ProfessionalFormModal
          title="Novo profissional"
          isPending={createMutation.isPending}
          onClose={() => setShowCreate(false)}
          onSubmit={(p) => createMutation.mutate(p, { onSuccess: () => setShowCreate(false) })}
        />
      )}

      {editItem && (
        <ProfessionalFormModal
          title="Editar profissional"
          initial={editItem}
          isPending={updateMutation.isPending}
          onClose={() => setEditItem(null)}
          onSubmit={(p) => updateMutation.mutate({ id: editItem.id, ...p }, { onSuccess: () => setEditItem(null) })}
        />
      )}
    </div>
  )
}

function ProfessionalFormModal({
  title,
  initial,
  isPending,
  onClose,
  onSubmit,
}: {
  title: string
  initial?: { name: string; email: string; phone: string }
  isPending: boolean
  onClose: () => void
  onSubmit: (payload: { name: string; email: string; phone: string }) => void
}) {
  const [name, setName] = useState(initial?.name ?? '')
  const [email, setEmail] = useState(initial?.email ?? '')
  const [phone, setPhone] = useState(initial?.phone ?? '')

  return (
    <FormModal title={title} onClose={onClose} isPending={isPending} onSubmit={(e) => { e.preventDefault(); onSubmit({ name, email, phone }) }}>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
        <input required value={name} onChange={(e) => setName(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
        <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Telefone</label>
        <input required value={phone} onChange={(e) => setPhone(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500" />
      </div>
    </FormModal>
  )
}
