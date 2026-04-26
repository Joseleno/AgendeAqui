import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { SkeletonCard, SkeletonChart } from '../../components/ui/Skeleton'
import { useTenantContext } from '../../context/TenantContext'
import { useTeamAppointmentsReport } from '../../hooks/useTeams'

export function EquipesReportPage() {
  const { labels } = useTenantContext()
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))

  const { data, isLoading } = useTeamAppointmentsReport(dateFrom, dateTo)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">
          Relatório de {labels.teams}
        </h2>
        <DateRangePicker
          dateFrom={dateFrom}
          dateTo={dateTo}
          onDateFromChange={setDateFrom}
          onDateToChange={setDateTo}
        />
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => <SkeletonCard key={i} />)}
          <SkeletonChart />
        </div>
      ) : data ? (
        <div className="space-y-6">
          {/* Summary cards */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <MetricCard label="Total de visitas" value={data.totalAppointments} color="text-brand-600" bg="bg-brand-50" />
            <MetricCard label="Concluídas" value={data.totalCompleted} color="text-green-600" bg="bg-green-50" />
            <MetricCard label="Canceladas" value={data.totalCancelled} color="text-red-600" bg="bg-red-50" />
            <MetricCard label="Ausências" value={data.totalNoShow} color="text-gray-600" bg="bg-gray-100" />
          </div>

          {/* Teams breakdown table */}
          {data.teams.length > 0 && (
            <div>
              <h3 className="text-sm font-semibold text-gray-700 mb-2">Por {labels.team}</h3>
              <div className="rounded-xl border border-gray-100 overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50 border-b border-gray-100">
                    <tr>
                      <th className="text-left px-4 py-2.5 font-medium text-gray-600">{labels.team}</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Total</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Concluídas</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Canceladas</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Taxa conclusão</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {data.teams.map((team) => (
                      <tr key={team.teamId} className="hover:bg-gray-50/50 transition-colors">
                        <td className="px-4 py-3 font-medium text-gray-800">{team.teamName}</td>
                        <td className="px-4 py-3 text-right text-gray-700">{team.total}</td>
                        <td className="px-4 py-3 text-right text-green-600">{team.completed}</td>
                        <td className="px-4 py-3 text-right text-red-500">{team.cancelled}</td>
                        <td className="px-4 py-3 text-right">
                          <span className={`font-medium ${team.completionRate >= 80 ? 'text-green-600' : team.completionRate >= 60 ? 'text-yellow-600' : 'text-red-500'}`}>
                            {(team.completionRate * 100).toFixed(1)}%
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Unassigned professionals */}
          {data.unassigned.length > 0 && (
            <div>
              <h3 className="text-sm font-semibold text-gray-700 mb-2">
                {labels.professionals} sem {labels.team}
              </h3>
              <div className="rounded-xl border border-gray-100 overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50 border-b border-gray-100">
                    <tr>
                      <th className="text-left px-4 py-2.5 font-medium text-gray-600">{labels.professional}</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Total</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Concluídas</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Canceladas</th>
                      <th className="text-right px-4 py-2.5 font-medium text-gray-600">Ausências</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {data.unassigned.map((p) => (
                      <tr key={p.professionalId} className="hover:bg-gray-50/50 transition-colors">
                        <td className="px-4 py-3 font-medium text-gray-800">{p.professionalName}</td>
                        <td className="px-4 py-3 text-right text-gray-700">{p.total}</td>
                        <td className="px-4 py-3 text-right text-green-600">{p.completed}</td>
                        <td className="px-4 py-3 text-right text-red-500">{p.cancelled}</td>
                        <td className="px-4 py-3 text-right text-gray-500">{p.noShow}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {data.teams.length === 0 && data.unassigned.length === 0 && (
            <p className="text-sm text-gray-400 text-center py-8">
              Nenhuma {labels.appointment.toLowerCase()} encontrada no período.
            </p>
          )}
        </div>
      ) : (
        <p className="text-sm text-gray-400">Selecione um período</p>
      )}
    </div>
  )
}

function MetricCard({ label, value, color, bg }: { label: string; value: number; color: string; bg: string }) {
  return (
    <div className={`${bg} rounded-xl p-4 border border-gray-100`}>
      <p className="text-sm text-gray-600">{label}</p>
      <p className={`text-2xl font-bold ${color} mt-1`}>{value}</p>
    </div>
  )
}
