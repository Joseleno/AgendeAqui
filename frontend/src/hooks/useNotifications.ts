import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import type { components } from '../api/schema'

export type Notification = components['schemas']['Notification']

export function useNotifications(dateFrom: string, dateTo: string) {
  return useQuery({
    queryKey: ['notifications', dateFrom, dateTo],
    queryFn: () => api.get<Notification[]>(`/notifications?dateFrom=${dateFrom}&dateTo=${dateTo}`),
    enabled: !!dateFrom && !!dateTo,
  })
}
