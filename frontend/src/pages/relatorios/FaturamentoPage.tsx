import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { RevenueChart } from '../../components/charts/RevenueChart'
import { RevenueTimelineChart } from '../../components/charts/RevenueTimelineChart'
import { ChartCard } from '../../components/charts/ChartCard'
import { useRevenueReport } from '../../hooks/useReports'
import { useRevenueTimeline } from '../../hooks/useAdvancedReports'
import { SkeletonChart, SkeletonTable } from '../../components/ui/Skeleton'
import { formatCurrency } from '../../lib/format'

export function FaturamentoPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))

  const { data, isLoading } = useRevenueReport(dateFrom, dateTo)
  const { data: timelineData, isLoading: timelineLoading } = useRevenueTimeline(dateFrom, dateTo)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Relatório de Faturamento</h2>
        <DateRangePicker
          dateFrom={dateFrom}
          dateTo={dateTo}
          onDateFromChange={setDateFrom}
          onDateToChange={setDateTo}
        />
      </div>

      {isLoading ? (
        <div className="space-y-4">
          <SkeletonChart />
          <SkeletonTable rows={3} cols={4} />
        </div>
      ) : data ? (
        <>
          <div className="bg-brand-50 rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-600">Faturamento total</p>
            <p className="text-2xl font-bold text-brand-600 mt-1">
              {formatCurrency(data.totalRevenue)}
            </p>
          </div>

          <ChartCard title="Evolução do faturamento" isLoading={timelineLoading}>
            {timelineData?.points && <RevenueTimelineChart data={timelineData.points} />}
          </ChartCard>

          {data.byService.length > 0 && (
            <ChartCard title="Por serviço">
              <RevenueChart data={data.byService} />
            </ChartCard>
          )}

          {data.byService.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                    <th className="px-4 py-3">Serviço</th>
                    <th className="px-4 py-3">Preço unit.</th>
                    <th className="px-4 py-3">Qtd</th>
                    <th className="px-4 py-3">Total</th>
                  </tr>
                </thead>
                <tbody>
                  {data.byService.map((s) => (
                    <tr key={s.serviceId} className="border-t border-gray-100">
                      <td className="px-4 py-3 font-medium text-gray-800">{s.serviceName}</td>
                      <td className="px-4 py-3 text-gray-600">{formatCurrency(s.unitPrice)}</td>
                      <td className="px-4 py-3 text-gray-600">{s.appointmentCount}</td>
                      <td className="px-4 py-3 font-medium text-gray-800">{formatCurrency(s.totalRevenue)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      ) : (
        <p className="text-sm text-gray-400">Selecione um período</p>
      )}
    </div>
  )
}
