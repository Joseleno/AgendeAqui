import { useState } from 'react'
import { Bell, CheckCheck } from 'lucide-react'
import { useInAppNotifications, useMarkAsRead, useMarkAllAsRead } from '../../hooks/useInAppNotifications'
import { NotificationItem } from './NotificationItem'
import { EmptyState } from '../ui/EmptyState'
import { SkeletonText } from '../ui/Skeleton'

interface NotificationPanelProps {
  onClose: () => void
}

export function NotificationPanel({ onClose }: NotificationPanelProps) {
  const [page] = useState(1)
  const { data, isLoading } = useInAppNotifications(page, 30)
  const markAsRead = useMarkAsRead()
  const markAllAsRead = useMarkAllAsRead()

  const notifications = data?.items ?? []
  const hasUnread = notifications.some((n) => !n.isRead)

  return (
    <>
      <div className="fixed inset-0 z-40" onClick={onClose} />
      <div className="absolute right-0 top-full mt-2 w-80 sm:w-96 bg-white rounded-xl border border-gray-200 shadow-modal z-50 animate-scale-in origin-top-right">
        <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
          <h3 className="text-sm font-semibold text-gray-800">Notificações</h3>
          {hasUnread && (
            <button
              onClick={() => markAllAsRead.mutate()}
              className="flex items-center gap-1 text-xs text-brand-600 hover:text-brand-800 font-medium"
              disabled={markAllAsRead.isPending}
            >
              <CheckCheck className="w-3.5 h-3.5" />
              Marcar tudo
            </button>
          )}
        </div>

        <div className="max-h-[60vh] overflow-y-auto p-1">
          {isLoading ? (
            <div className="space-y-3 p-3">
              {[1, 2, 3].map((i) => (
                <SkeletonText key={i} lines={2} />
              ))}
            </div>
          ) : notifications.length === 0 ? (
            <div className="py-6">
              <EmptyState
                icon={Bell}
                title="Sem notificações"
                subtitle="Você será notificado sobre seus agendamentos aqui"
              />
            </div>
          ) : (
            <div className="space-y-0.5">
              {notifications.map((n) => (
                <NotificationItem
                  key={n.id}
                  notification={n}
                  onClick={(notif) => {
                    if (!notif.isRead) markAsRead.mutate(notif.id)
                  }}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  )
}
