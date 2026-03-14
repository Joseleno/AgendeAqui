import { useState } from 'react'
import { useWebhooks, useCreateWebhook, useDeleteWebhook } from '../../hooks/useWebhooks'
import { FormModal } from '../ui/FormModal'
import { ConfirmDialog } from '../ui/ConfirmDialog'

const AVAILABLE_EVENTS = [
  'appointment.created',
  'appointment.cancelled',
  'appointment.rescheduled',
]

export function WebhookList() {
  const [showCreate, setShowCreate] = useState(false)
  const [deleteId, setDeleteId] = useState<string | null>(null)

  const { data: webhooks, isLoading } = useWebhooks()
  const deleteMutation = useDeleteWebhook()

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700">Webhooks</h3>
        <button
          onClick={() => setShowCreate(true)}
          className="text-xs font-semibold text-brand-600 hover:text-brand-800"
        >
          + Novo
        </button>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : !webhooks || webhooks.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhum webhook</p>
      ) : (
        <div className="space-y-2">
          {webhooks.map((wh) => (
            <div key={wh.id} className="bg-gray-50 rounded-lg px-3 py-2">
              <div className="flex items-center justify-between">
                <p className="text-sm font-medium text-gray-800 truncate">{wh.url}</p>
                <div className="flex items-center gap-2 shrink-0 ml-2">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${wh.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-200 text-gray-500'}`}>
                    {wh.isActive ? 'Ativo' : 'Inativo'}
                  </span>
                  <button
                    onClick={() => setDeleteId(wh.id)}
                    className="text-xs text-red-600 hover:text-red-800"
                  >
                    Remover
                  </button>
                </div>
              </div>
              <div className="flex flex-wrap gap-1 mt-1">
                {wh.events.map((ev) => (
                  <span key={ev} className="text-[10px] bg-brand-100 text-brand-700 px-1.5 py-0.5 rounded">
                    {ev}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {showCreate && <WebhookCreateModal onClose={() => setShowCreate(false)} />}

      {deleteId && (
        <ConfirmDialog
          title="Remover webhook"
          message="O webhook sera removido permanentemente. Continuar?"
          confirmLabel="Remover"
          isPending={deleteMutation.isPending}
          onConfirm={() => deleteMutation.mutate(deleteId, { onSuccess: () => setDeleteId(null) })}
          onCancel={() => setDeleteId(null)}
        />
      )}
    </div>
  )
}

function WebhookCreateModal({ onClose }: { onClose: () => void }) {
  const [url, setUrl] = useState('')
  const [secret, setSecret] = useState('')
  const [events, setEvents] = useState<string[]>([])

  const createMutation = useCreateWebhook()

  function toggleEvent(event: string) {
    setEvents((prev) =>
      prev.includes(event) ? prev.filter((e) => e !== event) : [...prev, event],
    )
  }

  return (
    <FormModal
      title="Novo webhook"
      onClose={onClose}
      isPending={createMutation.isPending}
      onSubmit={(e) => {
        e.preventDefault()
        createMutation.mutate({ url, secret, events }, { onSuccess: onClose })
      }}
    >
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">URL</label>
        <input
          type="url"
          required
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          placeholder="https://..."
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Secret</label>
        <input
          required
          value={secret}
          onChange={(e) => setSecret(e.target.value)}
          placeholder="whsec_..."
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Eventos</label>
        <div className="space-y-1">
          {AVAILABLE_EVENTS.map((ev) => (
            <label key={ev} className="flex items-center gap-2 text-sm text-gray-700">
              <input
                type="checkbox"
                checked={events.includes(ev)}
                onChange={() => toggleEvent(ev)}
                className="rounded border-gray-300"
              />
              {ev}
            </label>
          ))}
        </div>
      </div>
    </FormModal>
  )
}
