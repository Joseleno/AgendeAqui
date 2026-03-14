import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, Stethoscope } from 'lucide-react'
import { useSearchProfessionals } from '../../hooks/usePatientPortal'
import { PagedList } from '../../components/ui/PagedList'

export function BuscarProfissionalPage() {
  const navigate = useNavigate()
  const [name, setName] = useState('')
  const [specialty, setSpecialty] = useState('')
  const [debouncedName, setDebouncedName] = useState('')
  const [debouncedSpecialty, setDebouncedSpecialty] = useState('')
  const [page, setPage] = useState(1)

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedName(name)
      setDebouncedSpecialty(specialty)
    }, 300)
    return () => clearTimeout(t)
  }, [name, specialty])

  const { data, isLoading } = useSearchProfessionals(
    debouncedName || undefined,
    debouncedSpecialty || undefined,
    undefined,
    page,
  )

  const items = data?.items ?? []

  return (
    <div className="space-y-4 animate-fade-in">
      <h1 className="text-xl font-bold text-gray-800">Buscar Profissional</h1>

      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            value={name}
            onChange={(e) => { setName(e.target.value); setPage(1) }}
            placeholder="Nome do profissional..."
            className="w-full rounded-xl border border-gray-200 bg-gray-50/50 pl-9 pr-3 py-2.5 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
          />
        </div>
        <div className="relative">
          <Stethoscope className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            value={specialty}
            onChange={(e) => { setSpecialty(e.target.value); setPage(1) }}
            placeholder="Especialidade..."
            className="rounded-xl border border-gray-200 bg-gray-50/50 pl-9 pr-3 py-2.5 text-sm outline-none focus:bg-white focus:border-brand-500 focus:ring-1 focus:ring-brand-500 transition-all duration-200"
          />
        </div>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-36 bg-gray-100 rounded-xl animate-pulse" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <div className="text-center py-12">
          <Search className="w-10 h-10 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum profissional encontrado</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {items.map((prof) => (
            <button
              key={prof.id}
              onClick={() => navigate(`/agendar?professionalId=${prof.id}`)}
              className="bg-white rounded-xl border border-gray-200 p-4 text-left hover:shadow-card-hover hover:border-brand-200 transition-all duration-200"
            >
              <div className="flex items-center gap-3 mb-3">
                <div className="w-10 h-10 rounded-full bg-brand-100 flex items-center justify-center shrink-0">
                  <Stethoscope className="w-5 h-5 text-brand-600" />
                </div>
                <div className="min-w-0">
                  <p className="font-semibold text-gray-800 text-sm truncate">{prof.name}</p>
                  {prof.specialty && (
                    <p className="text-xs text-brand-600">{prof.specialty}</p>
                  )}
                </div>
              </div>
              {prof.services.length > 0 && (
                <div className="flex flex-wrap gap-1">
                  {prof.services.slice(0, 3).map((s) => (
                    <span key={s} className="bg-gray-100 text-gray-600 text-xs px-2 py-0.5 rounded-full">{s}</span>
                  ))}
                  {prof.services.length > 3 && (
                    <span className="text-xs text-gray-400">+{prof.services.length - 3}</span>
                  )}
                </div>
              )}
            </button>
          ))}
        </div>
      )}

      <PagedList page={page} totalPages={data?.totalPages ?? 1} onPageChange={setPage} />
    </div>
  )
}
