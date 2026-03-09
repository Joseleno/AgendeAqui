import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'

export interface Schedule {
  id: string
  professionalId: string
  professionalName: string
  dayOfWeek: number
  startTime: string
  endTime: string
  slotDurationMinutes: number
  isActive: boolean
  createdAt: string
}

interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export function useSchedules(page: number, professionalId?: string, pageSize = 10) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (professionalId) params.set('professionalId', professionalId)

  return useQuery({
    queryKey: ['schedules', page, professionalId, pageSize],
    queryFn: () => api.get<PagedResponse<Schedule>>(`/schedules?${params}`),
  })
}

export function useCreateSchedule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: {
      professionalId: string
      dayOfWeek: number
      startTime: string
      endTime: string
      slotDurationMinutes: number
    }) => api.post<{ id: string }>('/schedules', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedules'] })
      toast('success', 'Horario criado')
    },
    onError: () => toast('error', 'Erro ao criar horario'),
  })
}

export function useUpdateSchedule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string; startTime: string; endTime: string; slotDurationMinutes: number }) =>
      api.put(`/schedules/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedules'] })
      toast('success', 'Horario atualizado')
    },
    onError: () => toast('error', 'Erro ao atualizar horario'),
  })
}

export function useDeactivateSchedule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => api.post(`/schedules/${id}/deactivate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedules'] })
      toast('success', 'Horario desativado')
    },
    onError: () => toast('error', 'Erro ao desativar horario'),
  })
}
