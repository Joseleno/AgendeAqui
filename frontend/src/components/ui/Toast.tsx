import { useEffect, useState } from 'react'
import { CheckCircle, XCircle, Info, X } from 'lucide-react'

interface ToastMessage {
  id: string
  type: 'success' | 'error' | 'info'
  message: string
}

let addToast: (msg: Omit<ToastMessage, 'id'>) => void = () => {}

export function toast(type: ToastMessage['type'], message: string) {
  addToast({ type, message })
}

const icons = {
  success: CheckCircle,
  error: XCircle,
  info: Info,
}

const styles = {
  success: 'bg-emerald-50 text-emerald-800 border-emerald-200',
  error: 'bg-red-50 text-red-800 border-red-200',
  info: 'bg-blue-50 text-blue-800 border-blue-200',
}

export function ToastContainer() {
  const [toasts, setToasts] = useState<ToastMessage[]>([])

  useEffect(() => {
    addToast = (msg) => {
      const id = crypto.randomUUID()
      setToasts((prev) => [...prev, { ...msg, id }])
      setTimeout(() => {
        setToasts((prev) => prev.filter((t) => t.id !== id))
      }, 4000)
    }
  }, [])

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2">
      {toasts.map((t) => {
        const Icon = icons[t.type]
        return (
          <div
            key={t.id}
            className={`${styles[t.type]} flex items-center gap-2.5 px-4 py-3 rounded-xl border shadow-card animate-slide-in-right text-sm`}
          >
            <Icon className="w-4.5 h-4.5 shrink-0" />
            <span className="flex-1">{t.message}</span>
            <button
              onClick={() => setToasts((prev) => prev.filter((tt) => tt.id !== t.id))}
              className="text-current opacity-50 hover:opacity-100 transition-opacity"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          </div>
        )
      })}
    </div>
  )
}
