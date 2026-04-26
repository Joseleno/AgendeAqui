import { useState } from 'react'
import { Users, Plus, Trash2 } from 'lucide-react'
import { useTenantContext } from '../../../context/TenantContext'
import { useTeams, useCreateTeam, useDisbandTeam, useAddTeamMember } from '../../../hooks/useTeams'
import { useProfessionals } from '../../../hooks/useProfessionals'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { EmptyState } from '../../../components/ui/EmptyState'
import { SkeletonCard } from '../../../components/ui/Skeleton'
import { toast } from '../../../components/ui/Toast'

export function EquipesPage() {
  const { labels } = useTenantContext()
  const { data: teams, isLoading } = useTeams()
  const { data: profsData } = useProfessionals(1, 200)
  const createTeam = useCreateTeam()
  const disbandTeam = useDisbandTeam()
  const addMember = useAddTeamMember()

  const [showCreate, setShowCreate] = useState(false)
  const [newName, setNewName] = useState('')
  const [newDesc, setNewDesc] = useState('')
  const [disbandId, setDisbandId] = useState<string | null>(null)
  const [expandedTeam, setExpandedTeam] = useState<string | null>(null)
  const [selectedProfId, setSelectedProfId] = useState<Record<string, string>>({})

  const professionals = profsData?.items ?? []

  async function handleCreate() {
    if (!newName.trim()) return
    try {
      await createTeam.mutateAsync({ name: newName.trim(), description: newDesc.trim() || undefined })
      toast('success', `${labels.team} criada com sucesso.`)
      setShowCreate(false)
      setNewName('')
      setNewDesc('')
    } catch {
      toast('error', 'Erro ao criar equipe.')
    }
  }

  async function handleDisband(teamId: string) {
    try {
      await disbandTeam.mutateAsync(teamId)
      toast('success', `${labels.team} encerrada.`)
    } catch {
      toast('error', 'Erro ao encerrar equipe.')
    } finally {
      setDisbandId(null)
    }
  }

  async function handleAddMember(teamId: string) {
    const profId = selectedProfId[teamId]
    if (!profId) return
    try {
      await addMember.mutateAsync({ teamId, professionalId: profId })
      toast('success', `${labels.professional} adicionado(a).`)
      setSelectedProfId((prev) => ({ ...prev, [teamId]: '' }))
    } catch {
      toast('error', 'Erro ao adicionar membro.')
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-gray-800">{labels.teams}</h2>
        <button
          onClick={() => setShowCreate(true)}
          className="flex items-center gap-1.5 px-3 py-2 rounded-xl bg-brand-600 text-white text-sm font-medium hover:bg-brand-700 transition-colors"
        >
          <Plus className="w-4 h-4" />
          Nova {labels.team}
        </button>
      </div>

      {showCreate && (
        <div className="rounded-xl border border-brand-200 bg-brand-50/40 p-4 space-y-3">
          <h3 className="text-sm font-semibold text-gray-800">Nova {labels.team}</h3>
          <input
            autoFocus
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder={`Nome da ${labels.team.toLowerCase()}`}
            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
          />
          <input
            value={newDesc}
            onChange={(e) => setNewDesc(e.target.value)}
            placeholder="Descrição (opcional)"
            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-brand-500 focus:ring-1 focus:ring-brand-500"
          />
          <div className="flex gap-2">
            <button
              onClick={handleCreate}
              disabled={!newName.trim() || createTeam.isPending}
              className="px-4 py-2 rounded-lg bg-brand-600 text-white text-sm font-medium hover:bg-brand-700 disabled:opacity-50 transition-colors"
            >
              Criar
            </button>
            <button
              onClick={() => { setShowCreate(false); setNewName(''); setNewDesc('') }}
              className="px-4 py-2 rounded-lg border border-gray-200 text-sm text-gray-600 hover:bg-gray-50 transition-colors"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}

      {isLoading ? (
        <div className="space-y-3">{[1, 2].map((i) => <SkeletonCard key={i} />)}</div>
      ) : !teams?.length ? (
        <EmptyState
          icon={Users}
          title={`Nenhuma ${labels.team.toLowerCase()} criada`}
          subtitle={`Crie ${labels.teams.toLowerCase()} para organizar seus ${labels.professionals.toLowerCase()}.`}
        />
      ) : (
        <div className="space-y-3">
          {teams.map((team) => (
            <div key={team.id} className="rounded-xl border border-gray-100 bg-white overflow-hidden">
              <div
                className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-gray-50/50 transition-colors"
                onClick={() => setExpandedTeam(expandedTeam === team.id ? null : team.id)}
              >
                <div>
                  <p className="font-medium text-gray-800">{team.name}</p>
                  {team.description && (
                    <p className="text-xs text-gray-400 mt-0.5">{team.description}</p>
                  )}
                  <div className="flex items-center gap-3 mt-1">
                    <span className="text-xs text-gray-500">
                      {team.memberCount} {team.memberCount === 1 ? labels.professional.toLowerCase() : labels.professionals.toLowerCase()}
                    </span>
                    {team.leaderName && (
                      <span className="text-xs text-brand-600">Líder: {team.leaderName}</span>
                    )}
                  </div>
                </div>
                <button
                  onClick={(e) => { e.stopPropagation(); setDisbandId(team.id) }}
                  className="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 transition-colors"
                  title={`Encerrar ${labels.team.toLowerCase()}`}
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>

              {expandedTeam === team.id && (
                <div className="border-t border-gray-100 px-4 py-3 space-y-3">
                  <div className="flex items-center gap-2">
                    <select
                      value={selectedProfId[team.id] ?? ''}
                      onChange={(e) => setSelectedProfId((prev) => ({ ...prev, [team.id]: e.target.value }))}
                      className="flex-1 rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-brand-500"
                    >
                      <option value="">Adicionar {labels.professional.toLowerCase()}…</option>
                      {professionals.map((p) => (
                        <option key={p.id} value={p.id}>{p.name}</option>
                      ))}
                    </select>
                    <button
                      onClick={() => handleAddMember(team.id)}
                      disabled={!selectedProfId[team.id]}
                      className="px-3 py-2 rounded-lg bg-brand-600 text-white text-sm font-medium hover:bg-brand-700 disabled:opacity-40 transition-colors"
                    >
                      <Plus className="w-4 h-4" />
                    </button>
                  </div>
                  <p className="text-xs text-gray-400">
                    {team.memberCount === 0
                      ? `Nenhum ${labels.professional.toLowerCase()} nesta ${labels.team.toLowerCase()}.`
                      : `${team.memberCount} ${team.memberCount === 1 ? labels.professional.toLowerCase() : labels.professionals.toLowerCase()} nesta ${labels.team.toLowerCase()}.`}
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {disbandId && (
        <ConfirmDialog
          title={`Encerrar ${labels.team.toLowerCase()}`}
          message="Tem certeza? Esta ação não pode ser desfeita."
          confirmLabel="Encerrar"
          onConfirm={() => handleDisband(disbandId)}
          onCancel={() => setDisbandId(null)}
          isPending={disbandTeam.isPending}
        />
      )}
    </div>
  )
}
