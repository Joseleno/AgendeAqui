import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'

export interface Service {
  id: string
  name: string
  durationMinutes: number
  price: number
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

export function useServices(page: number, pageSize = 10) {
  return useQuery({
    queryKey: ['services', page, pageSize],
    queryFn: () => api.get<PagedResponse<Service>>(`/services?page=${page}&pageSize=${pageSize}`),
  })
}

export function useCreateService() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: { name: string; durationMinutes: number; price: number }) =>
      api.post<{ id: string }>('/services', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['services'] })
      toast('success', 'Servico criado')
    },
    onError: () => toast('error', 'Erro ao criar servico'),
  })
}

export function useUpdateService() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string; name: string; durationMinutes: number; price: number }) =>
      api.put(`/services/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['services'] })
      toast('success', 'Servico atualizado')
    },
    onError: () => toast('error', 'Erro ao atualizar servico'),
  })
}
