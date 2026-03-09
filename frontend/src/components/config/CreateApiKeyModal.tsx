import { useState } from 'react'
import { useCreateApiKey } from '../../hooks/useApiKeys'
import type { CreateApiKeyResponse } from '../../hooks/useApiKeys'
import { FormModal } from '../ui/FormModal'

interface CreateApiKeyModalProps {
  onClose: () => void
}

export function CreateApiKeyModal({ onClose }: CreateApiKeyModalProps) {
  const [name, setName] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [result, setResult] = useState<CreateApiKeyResponse | null>(null)

  const createMutation = useCreateApiKey()

  if (result) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center">
        <div className="absolute inset-0 bg-black/30" onClick={onClose} />
        <div className="relative bg-white rounded-xl shadow-xl p-5 max-w-md w-full mx-4">
          <h3 className="text-lg font-semibold text-gray-800 mb-3">API Key criada</h3>
          <p className="text-sm text-gray-600 mb-2">
            Copie a chave abaixo. Ela nao sera exibida novamente.
          </p>
          <div className="bg-gray-100 rounded-lg p-3 font-mono text-xs break-all select-all">
            {result.rawKey}
          </div>
          <button
            onClick={onClose}
            className="mt-4 w-full rounded-lg bg-indigo-600 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
          >
            Fechar
          </button>
        </div>
      </div>
    )
  }

  return (
    <FormModal
      title="Nova API Key"
      onClose={onClose}
      isPending={createMutation.isPending}
      onSubmit={(e) => {
        e.preventDefault()
        createMutation.mutate(
          { name, expiresAt: expiresAt || undefined },
          { onSuccess: (data) => setResult(data) },
        )
      }}
    >
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Nome</label>
        <input
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Ex: Integracao WhatsApp"
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Expira em (opcional)</label>
        <input
          type="datetime-local"
          value={expiresAt}
          onChange={(e) => setExpiresAt(e.target.value)}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-indigo-500"
        />
      </div>
    </FormModal>
  )
}
