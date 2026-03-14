import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { Trophy } from 'lucide-react'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { useProfessionalRanking } from '../../hooks/useDashboard'
import { RankingChart } from '../../components/charts/RankingChart'

export function RankingPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))

  const { data, isLoading } = useProfessionalRanking(dateFrom, dateTo)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Ranking de Profissionais</h2>
        <DateRangePicker dateFrom={dateFrom} dateTo={dateTo} onDateFromChange={setDateFrom} onDateToChange={setDateTo} />
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : !data || data.length === 0 ? (
        <div className="text-center py-12">
          <Trophy className="w-10 h-10 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-400">Nenhum dado encontrado</p>
        </div>
      ) : (
        <>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-3">Comparativo</h3>
            <RankingChart data={data} />
          </div>

          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                  <th className="px-4 py-3">#</th>
                  <th className="px-4 py-3">Profissional</th>
                  <th className="px-4 py-3 hidden sm:table-cell">Especialidade</th>
                  <th className="px-4 py-3">Total</th>
                  <th className="px-4 py-3">Concluídos</th>
                  <th className="px-4 py-3 hidden sm:table-cell">Cancelados</th>
                  <th className="px-4 py-3 hidden sm:table-cell">Faltas</th>
                  <th className="px-4 py-3">Taxa</th>
                </tr>
              </thead>
              <tbody>
                {data.map((r, i) => (
                  <tr key={r.professionalId} className="border-t border-gray-100 hover:bg-gray-50">
                    <td className="px-4 py-3 font-bold text-gray-400">{i + 1}</td>
                    <td className="px-4 py-3 font-medium text-gray-800">{r.name}</td>
                    <td className="px-4 py-3 text-gray-500 hidden sm:table-cell">{r.specialty ?? '-'}</td>
                    <td className="px-4 py-3 text-gray-600">{r.total}</td>
                    <td className="px-4 py-3 text-green-600 font-medium">{r.completed}</td>
                    <td className="px-4 py-3 text-red-600 hidden sm:table-cell">{r.cancelled}</td>
                    <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{r.noShow}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                        r.completionRate >= 80 ? 'bg-green-100 text-green-700' :
                        r.completionRate >= 50 ? 'bg-yellow-100 text-yellow-700' :
                        'bg-red-100 text-red-700'
                      }`}>
                        {r.completionRate}%
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}
