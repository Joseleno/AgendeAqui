import { useState } from 'react'
import { subDays, format } from 'date-fns'
import { useNotifications } from '../../hooks/useNotifications'
import { DateRangePicker } from '../../components/ui/DateRangePicker'

const statusStyle: Record<string, { bg: string; text: string }> = {
  Sent: { bg: 'bg-green-100', text: 'text-green-700' },
  Pending: { bg: 'bg-yellow-100', text: 'text-yellow-700' },
  Failed: { bg: 'bg-red-100', text: 'text-red-700' },
}

export function NotificacoesPage() {
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 7), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))

  const { data: notifications, isLoading } = useNotifications(dateFrom, dateTo)

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-gray-800">Historico de Notificacoes</h2>
        <DateRangePicker
          dateFrom={dateFrom}
          dateTo={dateTo}
          onDateFromChange={setDateFrom}
          onDateToChange={setDateTo}
        />
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : !notifications || notifications.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhuma notificacao no periodo</p>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                <th className="px-4 py-3">Canal</th>
                <th className="px-4 py-3 hidden sm:table-cell">Destinatario</th>
                <th className="px-4 py-3">Template</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3 hidden sm:table-cell">Data</th>
              </tr>
            </thead>
            <tbody>
              {notifications.map((n) => {
                const style = statusStyle[n.status] ?? { bg: 'bg-gray-100', text: 'text-gray-600' }
                return (
                  <tr key={n.id} className="border-t border-gray-100 hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-800">{n.channel}</td>
                    <td className="px-4 py-3 text-gray-600 hidden sm:table-cell truncate max-w-[200px]">{n.recipient}</td>
                    <td className="px-4 py-3 text-gray-600">{n.templateName}</td>
                    <td className="px-4 py-3">
                      <span className={`${style.bg} ${style.text} text-xs px-2 py-0.5 rounded-full`}>
                        {n.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500 text-xs hidden sm:table-cell">
                      {n.sentAt ? new Date(n.sentAt).toLocaleString('pt-BR') : '-'}
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
