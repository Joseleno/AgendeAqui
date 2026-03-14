import { useState } from 'react'
import { useApiKeys, useRevokeApiKey } from '../../hooks/useApiKeys'
import { CreateApiKeyModal } from './CreateApiKeyModal'
import { ConfirmDialog } from '../ui/ConfirmDialog'

export function ApiKeyList() {
  const [showCreate, setShowCreate] = useState(false)
  const [revokeId, setRevokeId] = useState<string | null>(null)

  const { data: keys, isLoading } = useApiKeys()
  const revokeMutation = useRevokeApiKey()

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700">API Keys</h3>
        <button
          onClick={() => setShowCreate(true)}
          className="text-xs font-semibold text-brand-600 hover:text-brand-800"
        >
          + Nova
        </button>
      </div>

      {isLoading ? (
        <p className="text-sm text-gray-400">Carregando...</p>
      ) : !keys || keys.length === 0 ? (
        <p className="text-sm text-gray-400">Nenhuma API key</p>
      ) : (
        <div className="space-y-2">
          {keys.map((key) => (
            <div key={key.id} className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2">
              <div>
                <p className="text-sm font-medium text-gray-800">{key.name}</p>
                <p className="text-xs text-gray-500 font-mono">{key.keyPrefix}...</p>
              </div>
              <div className="flex items-center gap-2">
                <span className={`text-xs px-2 py-0.5 rounded-full ${key.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-200 text-gray-500'}`}>
                  {key.isActive ? 'Ativa' : 'Revogada'}
                </span>
                {key.isActive && (
                  <button
                    onClick={() => setRevokeId(key.id)}
                    className="text-xs text-red-600 hover:text-red-800"
                  >
                    Revogar
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {showCreate && <CreateApiKeyModal onClose={() => setShowCreate(false)} />}

      {revokeId && (
        <ConfirmDialog
          title="Revogar API Key"
          message="A chave sera desativada permanentemente. Continuar?"
          confirmLabel="Revogar"
          isPending={revokeMutation.isPending}
          onConfirm={() => revokeMutation.mutate(revokeId, { onSuccess: () => setRevokeId(null) })}
          onCancel={() => setRevokeId(null)}
        />
      )}
    </div>
  )
}
