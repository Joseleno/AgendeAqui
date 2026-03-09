import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { RevenueChart } from '../../components/charts/RevenueChart'
import { useRevenueReport } from '../../hooks/useReports'

export function FaturamentoPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))

  const { data, isLoading } = useRevenueReport(dateFrom, dateTo)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Relatorio de Faturamento</h2>
        <DateRangePicker
          dateFrom={dateFrom}
          dateTo={dateTo}
          onDateFromChange={setDateFrom}
          onDateToChange={setDateTo}
        />
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : data ? (
        <>
          <div className="bg-indigo-50 rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-600">Faturamento total</p>
            <p className="text-2xl font-bold text-indigo-600 mt-1">
              R$ {data.totalRevenue.toFixed(2)}
            </p>
          </div>

          {data.byService.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-200 p-4">
              <h3 className="text-sm font-semibold text-gray-700 mb-3">Por servico</h3>
              <RevenueChart data={data.byService} />
            </div>
          )}

          {data.byService.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                    <th className="px-4 py-3">Servico</th>
                    <th className="px-4 py-3">Preco unit.</th>
                    <th className="px-4 py-3">Qtd</th>
                    <th className="px-4 py-3">Total</th>
                  </tr>
                </thead>
                <tbody>
                  {data.byService.map((s) => (
                    <tr key={s.serviceId} className="border-t border-gray-100">
                      <td className="px-4 py-3 font-medium text-gray-800">{s.serviceName}</td>
                      <td className="px-4 py-3 text-gray-600">R$ {s.unitPrice.toFixed(2)}</td>
                      <td className="px-4 py-3 text-gray-600">{s.appointmentCount}</td>
                      <td className="px-4 py-3 font-medium text-gray-800">R$ {s.totalRevenue.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      ) : (
        <p className="text-sm text-gray-400">Selecione um periodo</p>
      )}
    </div>
  )
}
