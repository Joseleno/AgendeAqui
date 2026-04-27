import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { Globe, Users, CalendarDays, TrendingUp, ChevronDown, Settings, ShieldOff, ShieldCheck, CreditCard, Link2 } from 'lucide-react'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { SkeletonCard, SkeletonChart } from '../../components/ui/Skeleton'
import { toast } from '../../components/ui/Toast'
import {
  usePlatformDashboard,
  useTenantUsage,
  useSuspendTenant,
  useActivateTenant,
  useChangeTenantPlan,
  useSetCustomDomain,
  type TenantAppointmentSummary,
} from '../../hooks/usePlatform'

const PLANS = ['Free', 'Starter', 'Professional', 'Enterprise']

export function PlataformaPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [managingId, setManagingId] = useState<string | null>(null)

  const { data, isLoading, error } = usePlatformDashboard(dateFrom, dateTo)

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <Globe className="w-6 h-6 text-brand-500" />
            Dashboard da Plataforma
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Visão cross-tenant — acesso exclusivo para operadores CodeProcess
          </p>
        </div>
        <DateRangePicker
          dateFrom={dateFrom}
          dateTo={dateTo}
          onDateFromChange={setDateFrom}
          onDateToChange={setDateTo}
        />
      </div>

      {error && (
        <div className="rounded-xl bg-red-50 border border-red-200 p-4 text-sm text-red-700">
          Acesso negado ou erro ao carregar dados da plataforma.
        </div>
      )}

      {isLoading ? (
        <div className="space-y-4">
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            {[1, 2, 3].map((i) => <SkeletonCard key={i} />)}
          </div>
          <SkeletonChart />
        </div>
      ) : data ? (
        <div className="space-y-6">
          {/* Platform-level metrics */}
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <SummaryCard
              label="Tenants ativos"
              value={data.activeTenants}
              total={data.totalTenants}
              icon={<Globe className="w-5 h-5 text-brand-500" />}
              bg="bg-brand-50"
            />
            <SummaryCard
              label="Total de agendamentos"
              value={data.totalAppointmentsInPeriod}
              icon={<CalendarDays className="w-5 h-5 text-blue-500" />}
              bg="bg-blue-50"
            />
            <SummaryCard
              label="Tenants"
              value={data.totalTenants}
              icon={<Users className="w-5 h-5 text-violet-500" />}
              bg="bg-violet-50"
            />
          </div>

          {/* Per-tenant breakdown table */}
          <div>
            <h2 className="text-base font-semibold text-gray-800 mb-3">Comparativo por tenant</h2>
            <div className="rounded-xl border border-gray-100 overflow-hidden">
              {data.tenants
                .slice()
                .sort((a, b) => b.total - a.total)
                .map((t, i, arr) => (
                  <TenantRow
                    key={t.tenantId}
                    tenant={t}
                    expanded={expandedId === t.tenantId}
                    managing={managingId === t.tenantId}
                    onToggle={() => setExpandedId(expandedId === t.tenantId ? null : t.tenantId)}
                    onManage={() => {
                      setManagingId(managingId === t.tenantId ? null : t.tenantId)
                      setExpandedId(null)
                    }}
                    isLast={i === arr.length - 1}
                  />
                ))}
            </div>
          </div>

          {/* Bar-style comparison chart */}
          {data.tenants.length > 1 && (
            <div>
              <h2 className="text-base font-semibold text-gray-800 mb-3 flex items-center gap-1.5">
                <TrendingUp className="w-4 h-4 text-brand-500" />
                Volume de agendamentos por tenant
              </h2>
              <div className="space-y-2">
                {data.tenants
                  .slice()
                  .sort((a, b) => b.total - a.total)
                  .map((t) => {
                    const max = Math.max(...data.tenants.map((x) => x.total), 1)
                    const pct = (t.total / max) * 100
                    return (
                      <div key={t.tenantId} className="flex items-center gap-3">
                        <span className="w-40 text-sm text-gray-700 truncate shrink-0">{t.tenantName}</span>
                        <div className="flex-1 h-5 bg-gray-100 rounded-full overflow-hidden">
                          <div
                            className="h-full bg-brand-500 rounded-full transition-all duration-500"
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                        <span className="w-10 text-right text-sm font-semibold text-gray-700 shrink-0">{t.total}</span>
                      </div>
                    )
                  })}
              </div>
            </div>
          )}
        </div>
      ) : null}
    </div>
  )
}

function TenantRow({
  tenant: t,
  expanded,
  managing,
  onToggle,
  onManage,
  isLast,
}: {
  tenant: TenantAppointmentSummary
  expanded: boolean
  managing: boolean
  onToggle: () => void
  onManage: () => void
  isLast: boolean
}) {
  const completionPct = t.total > 0 ? (t.completionRate * 100) : 0
  const completionColor = t.completionRate >= 0.8 ? 'text-green-600' : t.completionRate >= 0.6 ? 'text-yellow-600' : 'text-red-500'
  const barColor = t.completionRate >= 0.8 ? 'bg-green-500' : t.completionRate >= 0.6 ? 'bg-yellow-500' : 'bg-red-400'

  return (
    <div className={!isLast ? 'border-b border-gray-100' : ''}>
      <div className="flex items-center gap-2 px-4 py-3.5 hover:bg-gray-50/70 transition-colors">
        <button onClick={onToggle} className="flex-1 flex items-center gap-3 text-left min-w-0">
          <ChevronDown
            className={`w-4 h-4 text-gray-400 shrink-0 transition-transform duration-200 ${expanded ? 'rotate-180' : ''}`}
          />
          <div className="flex-1 min-w-0">
            <p className="font-medium text-gray-800 truncate">{t.tenantName}</p>
            <p className="text-xs text-gray-400 font-mono">{t.tenantSlug}</p>
          </div>
          <div className="flex items-center gap-4 shrink-0">
            <div className="hidden sm:flex items-center gap-1.5">
              <div className="w-20 h-1.5 rounded-full bg-gray-200 overflow-hidden">
                <div className={`h-full rounded-full transition-all ${barColor}`} style={{ width: `${Math.min(completionPct, 100)}%` }} />
              </div>
              <span className={`text-xs font-medium ${completionColor}`}>{completionPct.toFixed(0)}%</span>
            </div>
            <span className="text-sm font-semibold text-brand-700 w-8 text-right">{t.total}</span>
          </div>
        </button>
        <button
          onClick={onManage}
          title="Gerenciar tenant"
          className={`p-1.5 rounded-lg transition-colors ${managing ? 'bg-brand-100 text-brand-600' : 'text-gray-400 hover:text-gray-600 hover:bg-gray-100'}`}
        >
          <Settings className="w-4 h-4" />
        </button>
      </div>

      {expanded && (
        <div className="px-4 pb-4 pt-1 bg-gray-50/50 border-t border-gray-100 space-y-3">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <StatPill label="Agendamentos" value={t.total} color="text-brand-700" bg="bg-white" />
            <StatPill label="Concluídos" value={t.completed} color="text-green-600" bg="bg-white" />
            <StatPill label="Cancelados" value={t.cancelled} color="text-red-500" bg="bg-white" />
            <StatPill label="Não compareceu" value={t.noShow} color="text-gray-500" bg="bg-white" />
          </div>

          <div className="flex items-center gap-3">
            <div className="flex-1 h-2 rounded-full bg-gray-200 overflow-hidden flex">
              {t.total > 0 && (
                <>
                  <div className="h-full bg-green-500 transition-all" style={{ width: `${(t.completed / t.total) * 100}%` }} />
                  <div className="h-full bg-red-400 transition-all" style={{ width: `${(t.cancelled / t.total) * 100}%` }} />
                  <div className="h-full bg-gray-400 transition-all" style={{ width: `${(t.noShow / t.total) * 100}%` }} />
                </>
              )}
            </div>
            <span className={`text-sm font-semibold ${completionColor}`}>{completionPct.toFixed(1)}% conclusão</span>
          </div>

          <p className="text-xs text-gray-500">
            <span className="font-medium text-gray-700">{t.activeProfessionals}</span> profissional{t.activeProfessionals !== 1 ? 'is' : ''} ativo{t.activeProfessionals !== 1 ? 's' : ''}
          </p>
        </div>
      )}

      {managing && <ManagementPanel tenantId={t.tenantId} tenantSlug={t.tenantSlug} />}
    </div>
  )
}

function ManagementPanel({ tenantId, tenantSlug }: { tenantId: string; tenantSlug: string }) {
  const { data: usage, isLoading } = useTenantUsage(tenantId)
  const suspendMutation = useSuspendTenant()
  const activateMutation = useActivateTenant()
  const changePlanMutation = useChangeTenantPlan()
  const setDomainMutation = useSetCustomDomain()

  const [customDomain, setCustomDomain] = useState('')

  if (isLoading) return <div className="px-4 py-4 border-t border-brand-100 bg-brand-50/30 text-sm text-gray-400">Carregando...</div>
  if (!usage) return null

  const isPending = suspendMutation.isPending || activateMutation.isPending || changePlanMutation.isPending

  function formatLimit(val: number) {
    return val === -1 ? '∞' : val.toString()
  }

  return (
    <div className="px-4 py-4 border-t border-brand-100 bg-brand-50/30 space-y-4">
      {/* Status + Trial */}
      <div className="flex flex-wrap gap-2 items-center">
        <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${
          usage.status === 'Active' ? 'bg-green-100 text-green-700' :
          usage.status === 'Suspended' ? 'bg-red-100 text-red-700' :
          'bg-gray-100 text-gray-600'
        }`}>{usage.status}</span>
        <span className="text-xs bg-violet-100 text-violet-700 font-semibold px-2 py-0.5 rounded-full">{usage.plan}</span>
        {usage.isOnTrial && (
          <span className="text-xs bg-amber-100 text-amber-700 font-semibold px-2 py-0.5 rounded-full">
            Trial até {usage.trialEndsAt ? new Date(usage.trialEndsAt).toLocaleDateString('pt-BR') : '—'}
          </span>
        )}
        {usage.customDomain && (
          <span className="text-xs bg-blue-100 text-blue-700 font-mono px-2 py-0.5 rounded-full">{usage.customDomain}</span>
        )}
      </div>

      {/* Usage meters */}
      <div className="grid grid-cols-3 gap-3">
        <UsageMeter label="Profissionais" current={usage.professionalsCount} max={usage.maxProfessionals} />
        <UsageMeter label="Clientes" current={usage.clientsCount} max={usage.maxClients} />
        <UsageMeter label="Agend./mês" current={usage.appointmentsThisMonth} max={usage.maxAppointmentsPerMonth} />
      </div>

      {/* Actions row */}
      <div className="flex flex-wrap gap-2">
        {usage.status === 'Suspended' ? (
          <ActionButton
            icon={<ShieldCheck className="w-3.5 h-3.5" />}
            label="Ativar"
            color="green"
            loading={activateMutation.isPending}
            onClick={() => activateMutation.mutate(tenantId, {
              onSuccess: () => toast('success', `${tenantSlug} ativado`),
              onError: () => toast('error', 'Erro ao ativar'),
            })}
          />
        ) : (
          <ActionButton
            icon={<ShieldOff className="w-3.5 h-3.5" />}
            label="Suspender"
            color="red"
            loading={suspendMutation.isPending}
            onClick={() => suspendMutation.mutate(tenantId, {
              onSuccess: () => toast('success', `${tenantSlug} suspenso`),
              onError: () => toast('error', 'Erro ao suspender'),
            })}
          />
        )}

        <div className="flex items-center gap-1">
          <CreditCard className="w-3.5 h-3.5 text-gray-500" />
          <select
            defaultValue={usage.plan}
            disabled={isPending}
            onChange={(e) => changePlanMutation.mutate({ tenantId, plan: e.target.value }, {
              onSuccess: () => toast('success', `Plano alterado para ${e.target.value}`),
              onError: () => toast('error', 'Erro ao alterar plano'),
            })}
            className="text-xs rounded-lg border border-gray-300 px-2 py-1 outline-none focus:border-brand-500 disabled:opacity-50"
          >
            {PLANS.map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
        </div>
      </div>

      {/* Custom domain */}
      <div className="flex items-center gap-2">
        <Link2 className="w-3.5 h-3.5 text-gray-400 shrink-0" />
        <input
          value={customDomain}
          onChange={(e) => setCustomDomain(e.target.value)}
          placeholder={usage.customDomain ?? 'dominio.com.br'}
          className="flex-1 text-xs rounded-lg border border-gray-300 px-2 py-1 outline-none focus:border-brand-500 font-mono"
        />
        <button
          onClick={() => setDomainMutation.mutate({ tenantId, domain: customDomain || null }, {
            onSuccess: () => { toast('success', 'Domínio salvo'); setCustomDomain('') },
            onError: () => toast('error', 'Erro ao salvar domínio'),
          })}
          disabled={setDomainMutation.isPending}
          className="text-xs rounded-lg bg-brand-600 px-2.5 py-1 text-white hover:bg-brand-700 disabled:opacity-50 transition-colors"
        >
          Salvar
        </button>
        {usage.customDomain && (
          <button
            onClick={() => setDomainMutation.mutate({ tenantId, domain: null }, {
              onSuccess: () => toast('success', 'Domínio removido'),
              onError: () => toast('error', 'Erro'),
            })}
            className="text-xs text-red-500 hover:text-red-700"
          >
            Remover
          </button>
        )}
      </div>
    </div>
  )
}

function UsageMeter({ label, current, max }: { label: string; current: number; max: number }) {
  const isUnlimited = max === -1
  const pct = isUnlimited ? 0 : Math.min((current / max) * 100, 100)
  const color = pct > 90 ? 'bg-red-400' : pct > 70 ? 'bg-amber-400' : 'bg-brand-500'

  return (
    <div className="bg-white rounded-lg border border-gray-100 px-2.5 py-2">
      <p className="text-xs text-gray-500 mb-1">{label}</p>
      <p className="text-sm font-bold text-gray-800">{current}{isUnlimited ? '' : `/${max}`}</p>
      {!isUnlimited && (
        <div className="mt-1.5 h-1 bg-gray-100 rounded-full overflow-hidden">
          <div className={`h-full rounded-full ${color}`} style={{ width: `${pct}%` }} />
        </div>
      )}
    </div>
  )
}

function ActionButton({
  icon, label, color, loading, onClick,
}: {
  icon: React.ReactNode
  label: string
  color: 'red' | 'green'
  loading: boolean
  onClick: () => void
}) {
  const cls = color === 'red'
    ? 'bg-red-50 text-red-600 border-red-200 hover:bg-red-100'
    : 'bg-green-50 text-green-700 border-green-200 hover:bg-green-100'

  return (
    <button
      onClick={onClick}
      disabled={loading}
      className={`flex items-center gap-1.5 text-xs font-medium border rounded-lg px-2.5 py-1 disabled:opacity-50 transition-colors ${cls}`}
    >
      {icon}
      {loading ? '...' : label}
    </button>
  )
}

function StatPill({ label, value, color, bg }: { label: string; value: number; color: string; bg: string }) {
  return (
    <div className={`${bg} rounded-lg border border-gray-100 px-3 py-2`}>
      <p className="text-xs text-gray-500">{label}</p>
      <p className={`text-lg font-bold ${color}`}>{value}</p>
    </div>
  )
}

function SummaryCard({
  label, value, total, icon, bg,
}: {
  label: string
  value: number
  total?: number
  icon: React.ReactNode
  bg: string
}) {
  return (
    <div className={`${bg} rounded-xl p-4 border border-gray-100`}>
      <div className="flex items-center justify-between mb-2">
        <p className="text-sm text-gray-600">{label}</p>
        {icon}
      </div>
      <p className="text-2xl font-bold text-gray-900">{value}</p>
      {total !== undefined && (
        <p className="text-xs text-gray-400 mt-0.5">de {total} total</p>
      )}
    </div>
  )
}
