import { useState } from 'react'
import { format, subDays } from 'date-fns'
import { DollarSign, TrendingUp, Clock, RotateCcw } from 'lucide-react'
import { usePayments, usePaymentSummary, useUpdatePaymentStatus } from '../../hooks/usePayments'
import { PaymentBadge } from '../../components/payments/PaymentBadge'
import { DateRangePicker } from '../../components/ui/DateRangePicker'
import { PagedList } from '../../components/ui/PagedList'
import { EmptyState } from '../../components/ui/EmptyState'
import { SkeletonCard, SkeletonTable } from '../../components/ui/Skeleton'
import { formatCurrency } from '../../lib/format'
import { useAuthState } from '../../hooks/useAuth'
import { PAYMENT_METHOD_LABELS } from '../../lib/constants'

export function FinanceiroPage() {
  const [page, setPage] = useState(1)
  const [dateFrom, setDateFrom] = useState(format(subDays(new Date(), 30), 'yyyy-MM-dd'))
  const [dateTo, setDateTo] = useState(format(new Date(), 'yyyy-MM-dd'))
  const { isAdmin } = useAuthState()

  const { data: summary, isLoading: summaryLoading } = usePaymentSummary(dateFrom, dateTo)
  const { data: payments, isLoading: paymentsLoading } = usePayments(page, 20, undefined, dateFrom, dateTo)
  const updateStatus = useUpdatePaymentStatus()

  return (
    <div className="space-y-6 page-enter">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-gray-800">Financeiro</h1>
        <DateRangePicker
          dateFrom={dateFrom}
          dateTo={dateTo}
          onDateFromChange={setDateFrom}
          onDateToChange={setDateTo}
        />
      </div>

      {/* Summary Cards */}
      {summaryLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : summary ? (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 stagger-children">
          <SummaryCard icon={TrendingUp} label="Recebido" value={formatCurrency(summary.totalReceived)} color="text-green-600" bg="bg-green-50" />
          <SummaryCard icon={Clock} label="Pendente" value={formatCurrency(summary.totalPending)} color="text-amber-600" bg="bg-amber-50" />
          <SummaryCard icon={RotateCcw} label="Estornado" value={formatCurrency(summary.totalRefunded)} color="text-gray-600" bg="bg-gray-100" />
          <SummaryCard icon={DollarSign} label="Total pgtos" value={String(summary.paymentCount)} color="text-brand-600" bg="bg-brand-50" />
        </div>
      ) : null}

      {/* By Method */}
      {summary && summary.byMethod.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <h3 className="text-sm font-semibold text-gray-700 mb-3">Por método</h3>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            {summary.byMethod.map((m) => (
              <div key={m.method} className="bg-gray-50 rounded-lg p-3 text-center">
                <p className="text-xs text-gray-500 mb-1">{PAYMENT_METHOD_LABELS[m.method] ?? m.method}</p>
                <p className="text-lg font-bold text-gray-800">{formatCurrency(m.total)}</p>
                <p className="text-[10px] text-gray-400">{m.count} pgtos</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Payments List */}
      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Pagamentos recentes</h3>
        {paymentsLoading ? (
          <SkeletonTable rows={5} cols={5} />
        ) : !payments || payments.items.length === 0 ? (
          <EmptyState
            icon={DollarSign}
            title="Nenhum pagamento"
            subtitle="Os pagamentos registrados aparecerão aqui"
          />
        ) : (
          <>
            <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 text-left text-xs text-gray-500 uppercase">
                    <th className="px-4 py-3">Cliente</th>
                    <th className="px-4 py-3 hidden sm:table-cell">Serviço</th>
                    <th className="px-4 py-3">Valor</th>
                    <th className="px-4 py-3 hidden sm:table-cell">Método</th>
                    <th className="px-4 py-3">Status</th>
                    {isAdmin && <th className="px-4 py-3 w-20" />}
                  </tr>
                </thead>
                <tbody>
                  {payments.items.map((p) => (
                    <tr key={p.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-800">{p.clientName}</td>
                      <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{p.serviceName}</td>
                      <td className="px-4 py-3 font-medium text-gray-800">{formatCurrency(p.amount)}</td>
                      <td className="px-4 py-3 text-gray-600 hidden sm:table-cell">{PAYMENT_METHOD_LABELS[p.method] ?? p.method}</td>
                      <td className="px-4 py-3">
                        <PaymentBadge status={p.status} />
                      </td>
                      {isAdmin && (
                        <td className="px-4 py-3">
                          {p.status === 'Pending' && (
                            <button
                              onClick={() => updateStatus.mutate({ id: p.id, status: 'Paid' })}
                              className="text-xs text-green-600 hover:text-green-800 font-medium"
                            >
                              Confirmar
                            </button>
                          )}
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <PagedList page={page} totalPages={payments.totalPages} onPageChange={setPage} />
          </>
        )}
      </div>
    </div>
  )
}

function SummaryCard({ icon: Icon, label, value, color, bg }: {
  icon: typeof DollarSign; label: string; value: string; color: string; bg: string
}) {
  return (
    <div className={`${bg} rounded-xl p-4 border border-gray-100`}>
      <div className="flex items-center justify-between">
        <p className="text-xs font-medium text-gray-500">{label}</p>
        <Icon className={`w-4 h-4 ${color}`} />
      </div>
      <p className={`text-xl font-bold ${color} mt-2`}>{value}</p>
    </div>
  )
}
