import { useEffect, useState } from 'react'

interface ToastMessage {
  id: string
  type: 'success' | 'error' | 'info'
  message: string
}

let addToast: (msg: Omit<ToastMessage, 'id'>) => void = () => {}

export function toast(type: ToastMessage['type'], message: string) {
  addToast({ type, message })
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

  const colors = {
    success: 'bg-green-500',
    error: 'bg-red-500',
    info: 'bg-blue-500',
  }

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={`${colors[t.type]} text-white px-4 py-2 rounded-lg shadow-lg text-sm animate-slide-in`}
        >
          {t.message}
        </div>
      ))}
    </div>
  )
}
