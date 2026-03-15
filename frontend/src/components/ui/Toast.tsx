import { useEffect, useState, useCallback } from 'react'
import { CheckCircle, XCircle, Info, AlertTriangle, X } from 'lucide-react'

interface ToastMessage {
  id: string
  type: 'success' | 'error' | 'info' | 'warning'
  message: string
  detail?: string
}

const TOAST_DURATION = 4000

let addToast: (msg: Omit<ToastMessage, 'id'>) => void = () => {}

export function toast(type: ToastMessage['type'], message: string, detail?: string) {
  addToast({ type, message, detail })
}

const icons = {
  success: CheckCircle,
  error: XCircle,
  info: Info,
  warning: AlertTriangle,
}

const styles = {
  success: 'bg-emerald-50 text-emerald-800 border-emerald-200',
  error: 'bg-red-50 text-red-800 border-red-200',
  info: 'bg-blue-50 text-blue-800 border-blue-200',
  warning: 'bg-amber-50 text-amber-800 border-amber-200',
}

const progressColors = {
  success: 'bg-emerald-400',
  error: 'bg-red-400',
  info: 'bg-blue-400',
  warning: 'bg-amber-400',
}

function ToastItem({ toast: t, onDismiss }: { toast: ToastMessage; onDismiss: (id: string) => void }) {
  const [exiting, setExiting] = useState(false)
  const Icon = icons[t.type]

  const dismiss = useCallback(() => {
    setExiting(true)
    setTimeout(() => onDismiss(t.id), 200)
  }, [onDismiss, t.id])

  useEffect(() => {
    const timer = setTimeout(dismiss, TOAST_DURATION)
    return () => clearTimeout(timer)
  }, [dismiss])

  return (
    <div
      className={`${styles[t.type]} relative flex items-start gap-2.5 px-4 py-3 rounded-xl border shadow-card text-sm overflow-hidden transition-all duration-200 ${
        exiting ? 'opacity-0 translate-x-full' : 'animate-slide-in-right'
      }`}
    >
      <Icon className="w-4.5 h-4.5 shrink-0 mt-0.5" />
      <div className="flex-1 min-w-0">
        <span className="font-medium">{t.message}</span>
        {t.detail && (
          <p className="text-xs opacity-75 mt-0.5">{t.detail}</p>
        )}
      </div>
      <button
        onClick={dismiss}
        className="text-current opacity-50 hover:opacity-100 transition-opacity shrink-0"
        aria-label="Fechar"
      >
        <X className="w-3.5 h-3.5" />
      </button>
      <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-black/5">
        <div
          className={`h-full ${progressColors[t.type]} rounded-full`}
          style={{
            animation: `toast-progress ${TOAST_DURATION}ms linear forwards`,
          }}
        />
      </div>
    </div>
  )
}

export function ToastContainer() {
  const [toasts, setToasts] = useState<ToastMessage[]>([])

  useEffect(() => {
    addToast = (msg) => {
      const id = crypto.randomUUID()
      setToasts((prev) => [...prev, { ...msg, id }])
    }
    return () => { addToast = () => {} }
  }, [])

  const handleDismiss = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }, [])

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2 max-w-sm w-full pointer-events-none">
      {toasts.map((t) => (
        <div key={t.id} className="pointer-events-auto">
          <ToastItem toast={t} onDismiss={handleDismiss} />
        </div>
      ))}
    </div>
  )
}
