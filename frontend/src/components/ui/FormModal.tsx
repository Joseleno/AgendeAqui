import type { ReactNode } from 'react'

interface FormModalProps {
  title: string
  onClose: () => void
  onSubmit: (e: React.FormEvent) => void
  isPending?: boolean
  submitLabel?: string
  children: ReactNode
}

export function FormModal({
  title,
  onClose,
  onSubmit,
  isPending = false,
  submitLabel = 'Salvar',
  children,
}: FormModalProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center">
      <div className="absolute inset-0 bg-black/30" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-md md:rounded-xl rounded-t-xl shadow-xl p-5 max-h-[80vh] overflow-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-800">{title}</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl">&times;</button>
        </div>
        <form onSubmit={onSubmit} className="space-y-3">
          {children}
          <button
            type="submit"
            disabled={isPending}
            className="w-full rounded-lg bg-indigo-600 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            {isPending ? 'Salvando...' : submitLabel}
          </button>
        </form>
      </div>
    </div>
  )
}
