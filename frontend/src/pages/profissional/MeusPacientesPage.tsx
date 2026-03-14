import { useState } from 'react'
import { Users } from 'lucide-react'
import { SearchInput } from '../../components/ui/SearchInput'
import { PagedList } from '../../components/ui/PagedList'
import { useMyPatients } from '../../hooks/useMyPatients'

export function MeusPacientesPage() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')

  function handleSearch(value: string) {
    setSearch(value)
    setPage(1)
  }

  const { data, isLoading } = useMyPatients(page, 10, search || undefined)
  const items = data?.items ?? []

  return (
    <div className="space-y-4 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Meus Pacientes</h1>
        <SearchInput value={search} onChange={handleSearch} placeholder="Buscar paciente..." />
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-16 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <div className="text-center py-12">
          <Users className="w-10 h-10 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum paciente encontrado</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Nome</th>
                <th className="px-4 py-3 hidden sm:table-cell">Email</th>
                <th className="px-4 py-3 hidden sm:table-cell">Telefone</th>
                <th className="px-4 py-3">Último atend.</th>
                <th className="px-4 py-3">Total</th>
              </tr>
            </thead>
            <tbody>
              {items.map((p) => (
                <tr key={p.clientId} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-800">{p.name}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{p.email}</td>
                  <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{p.phone}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">{p.lastAppointmentDate ?? '-'}</td>
                  <td className="px-4 py-3">
                    <span className="bg-brand-100 text-brand-700 text-xs px-2 py-0.5 rounded-full font-medium">
                      {p.totalAppointments}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <PagedList page={page} totalPages={data?.totalPages ?? 1} onPageChange={setPage} />
    </div>
  )
}
