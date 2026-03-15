import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface InAppNotification {
  id: string
  title: string
  message: string
  type: string
  referenceId: string | null
  isRead: boolean
  createdAt: string
}

export interface PagedNotifications {
  items: InAppNotification[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export function useInAppNotifications(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['notifications', 'in-app', page, pageSize],
    queryFn: () => api.get<PagedNotifications>(`/notifications/in-app?page=${page}&pageSize=${pageSize}`),
  })
}

export function useUnreadCount() {
  return useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: () => api.get<number>('/notifications/in-app/unread-count'),
    refetchInterval: 30_000,
    staleTime: 30_000,
  })
}

export function useMarkAsRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.put(`/notifications/in-app/${id}/read`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
    },
  })
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => api.put('/notifications/in-app/read-all'),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
    },
  })
}
