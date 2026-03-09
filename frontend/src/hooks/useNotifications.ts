import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface Notification {
  id: string
  appointmentId: string
  channel: string
  recipient: string
  templateName: string
  status: string
  sentAt: string | null
  errorMessage: string | null
  createdAt: string
}

export function useNotifications(dateFrom: string, dateTo: string) {
  return useQuery({
    queryKey: ['notifications', dateFrom, dateTo],
    queryFn: () => api.get<Notification[]>(`/notifications?dateFrom=${dateFrom}&dateTo=${dateTo}`),
    enabled: !!dateFrom && !!dateTo,
  })
}
