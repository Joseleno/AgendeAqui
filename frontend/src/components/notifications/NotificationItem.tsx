import { CalendarPlus, CalendarX, CalendarClock, CalendarCheck, AlertTriangle } from 'lucide-react'
import type { InAppNotification } from '../../hooks/useInAppNotifications'

interface NotificationItemProps {
  notification: InAppNotification
  onClick: (notification: InAppNotification) => void
}

const typeIcons: Record<string, typeof CalendarPlus> = {
  AppointmentCreated: CalendarPlus,
  AppointmentCancelled: CalendarX,
  AppointmentRescheduled: CalendarClock,
  AppointmentCompleted: CalendarCheck,
  AppointmentNoShow: AlertTriangle,
  AppointmentReminder: CalendarClock,
}

const typeColors: Record<string, string> = {
  AppointmentCreated: 'text-brand-600 bg-brand-50',
  AppointmentCancelled: 'text-red-600 bg-red-50',
  AppointmentRescheduled: 'text-blue-600 bg-blue-50',
  AppointmentCompleted: 'text-green-600 bg-green-50',
  AppointmentNoShow: 'text-amber-600 bg-amber-50',
  AppointmentReminder: 'text-purple-600 bg-purple-50',
}

function timeAgo(dateStr: string): string {
  const now = Date.now()
  const date = new Date(dateStr).getTime()
  const diff = Math.floor((now - date) / 1000)
  if (diff < 60) return 'agora'
  if (diff < 3600) return `${Math.floor(diff / 60)}min`
  if (diff < 86400) return `${Math.floor(diff / 3600)}h`
  return `${Math.floor(diff / 86400)}d`
}

export function NotificationItem({ notification, onClick }: NotificationItemProps) {
  const Icon = typeIcons[notification.type] ?? CalendarPlus
  const colorClass = typeColors[notification.type] ?? 'text-gray-600 bg-gray-50'

  return (
    <button
      onClick={() => onClick(notification)}
      className={`w-full text-left flex items-start gap-3 px-3 py-2.5 rounded-lg transition-colors ${
        notification.isRead ? 'hover:bg-gray-50' : 'bg-brand-50/30 hover:bg-brand-50/50'
      }`}
    >
      <div className={`w-8 h-8 rounded-lg flex items-center justify-center shrink-0 ${colorClass}`}>
        <Icon className="w-4 h-4" />
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className={`text-sm truncate ${notification.isRead ? 'text-gray-700' : 'text-gray-900 font-medium'}`}>
            {notification.title}
          </p>
          {!notification.isRead && (
            <div className="w-2 h-2 rounded-full bg-brand-500 shrink-0" />
          )}
        </div>
        <p className="text-xs text-gray-500 truncate">{notification.message}</p>
        <p className="text-[10px] text-gray-400 mt-0.5">{timeAgo(notification.createdAt)}</p>
      </div>
    </button>
  )
}
