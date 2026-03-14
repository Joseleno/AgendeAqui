import type { ReactNode } from 'react'
import { X } from 'lucide-react'

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
    <div className="fixed inset-0 z-50 flex items-end md:items-center justify-center animate-fade-in">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-white w-full md:max-w-md md:rounded-2xl rounded-t-2xl shadow-modal p-6 max-h-[85vh] overflow-auto animate-slide-up">
        <div className="flex items-center justify-between mb-5">
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
          <button
            onClick={onClose}
            className="p-1 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
        <form onSubmit={onSubmit} className="space-y-4">
          {children}
          <button
            type="submit"
            disabled={isPending}
            className="w-full rounded-xl bg-brand-600 py-2.5 text-sm font-semibold text-white hover:bg-brand-700 disabled:opacity-50 transition-all duration-200 shadow-sm hover:shadow-md active:scale-[0.98]"
          >
            {isPending ? 'Salvando...' : submitLabel}
          </button>
        </form>
      </div>
    </div>
  )
}
