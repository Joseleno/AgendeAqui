import { useState } from 'react'
import { useClients, useCreateClient, useUpdateClient } from '../../../hooks/useClients'
import type { Client } from '../../../hooks/useClients'
import { SearchInput } from '../../../components/ui/SearchInput'
import { PagedList } from '../../../components/ui/PagedList'
import { FormModal } from '../../../components/ui/FormModal'

export function ClientesPage() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [editClient, setEditClient] = useState<Client | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  const { data, isLoading } = useClients(page)
  const createMutation = useCreateClient()
  const updateMutation = useUpdateClient()

  const clients = (data?.items ?? []).filter(
    (c) => !search || c.name.toLowerCase().includes(search.toLowerCase()) || c.email.toLowerCase().includes(search.toLowerCase()),
  )

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-800">Clientes</h1>
        <button
          onClick={() => setShowCreate(true)}
          className="rounded-lg bg-indigo-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-indigo-700"
        >
          + Novo
        </button>
      </div>

      <SearchInput value={search} onChange={setSearch} placeholder="Buscar cliente..." />

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : clients.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum cliente encontrado</p>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Nome</th>
                <th className="px-4 py-3 hidden sm:table-cell">Email</th>
                <th className="px-4 py-3 hidden sm:table-cell">Telefone</th>
                <th className="px-4 py-3 w-20"></th>
              </tr>
            </thead>
            <tbody>
              {clients.map((c) => (
                <tr key={c.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{c.name}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{c.email}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{c.phone}</td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => setEditClient(c)}
                      className="text-xs text-indigo-600 hover:text-indigo-800"
                    >
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
        <ClientFormModal
          title="Novo cliente"
          isPending={createMutation.isPending}
          onClose={() => setShowCreate(false)}
          onSubmit={(payload) => createMutation.mutate(payload, { onSuccess: () => setShowCreate(false) })}
        />
      )}

      {editClient && (
        <ClientFormModal
          title="Editar cliente"
          initial={editClient}
          isPending={updateMutation.isPending}
          onClose={() => setEditClient(null)}
          onSubmit={(payload) =>
            updateMutation.mutate({ id: editClient.id, ...payload }, { onSuccess: () => setEditClient(null) })
          }
        />
      )}
    </div>
  )
}

function ClientFormModal({
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
        <input required value={name} onChange={(e) => setName(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
        <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500" />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Telefone</label>
        <input required value={phone} onChange={(e) => setPhone(e.target.value)} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500" />
      </div>
    </FormModal>
  )
}
