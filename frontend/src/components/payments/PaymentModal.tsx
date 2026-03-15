import { useState } from 'react'
import { useCreatePayment } from '../../hooks/usePayments'
import { PAYMENT_METHOD_LABELS } from '../../lib/constants'

interface PaymentModalProps {
  appointmentId: string
  defaultAmount?: number
  onClose: () => void
}

const methods = Object.entries(PAYMENT_METHOD_LABELS).map(([value, label]) => ({ value, label }))

export function PaymentModal({ appointmentId, defaultAmount, onClose }: PaymentModalProps) {
  const [amount, setAmount] = useState(defaultAmount?.toString() ?? '')
  const [method, setMethod] = useState('Pix')
  const [notes, setNotes] = useState('')
  const createPayment = useCreatePayment()

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const parsedAmount = parseFloat(amount)
    if (isNaN(parsedAmount) || parsedAmount <= 0) return
    createPayment.mutate(
      { appointmentId, amount: parsedAmount, method, notes: notes || undefined },
      { onSuccess: onClose },
    )
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center" role="dialog" aria-modal="true" aria-labelledby="payment-modal-title" tabIndex={-1} onKeyDown={(e) => { if (e.key === 'Escape') onClose() }}>
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-sm md:rounded-xl rounded-t-xl shadow-xl p-5">
        <div className="flex items-center justify-between mb-4">
          <h3 id="payment-modal-title" className="text-lg font-semibold text-gray-800">Registrar pagamento</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Valor (R$)</label>
            <input
              type="number"
              required
              min="0.01"
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
              placeholder="0,00"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Forma de pagamento</label>
            <div className="grid grid-cols-2 gap-2">
              {methods.map((m) => (
                <button
                  key={m.value}
                  type="button"
                  onClick={() => setMethod(m.value)}
                  className={`px-3 py-2 rounded-lg text-sm font-medium border transition-colors ${
                    method === m.value
                      ? 'border-brand-500 bg-brand-50 text-brand-700'
                      : 'border-gray-300 text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  {m.label}
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Observação</label>
            <input
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
              placeholder="Opcional"
            />
          </div>

          <div className="flex gap-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 rounded-lg border border-gray-300 py-2 text-sm text-gray-600 hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={createPayment.isPending}
              className="flex-1 rounded-lg bg-brand-600 py-2 text-sm text-white font-medium hover:bg-brand-700 disabled:opacity-50"
            >
              {createPayment.isPending ? 'Salvando...' : 'Registrar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
