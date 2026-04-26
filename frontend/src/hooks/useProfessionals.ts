import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

export type Professional = components['schemas']['Professional']
type PagedResponse<T> = { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }

export function useProfessionals(page: number, pageSize = 10) {
  return useQuery({
    queryKey: ['professionals', page, pageSize],
    queryFn: () => api.get<PagedResponse<Professional>>(`/professionals?page=${page}&pageSize=${pageSize}`),
  })
}

export function useCreateProfessional() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: { name: string; email: string; phone: string }) =>
      api.post<{ id: string }>('/professionals', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['professionals'] })
      toast('success', 'Profissional criado')
    },
    onError: () => toast('error', 'Erro ao criar profissional'),
  })
}

export function useUpdateProfessional() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string; name: string; email: string; phone: string }) =>
      api.put(`/professionals/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['professionals'] })
      toast('success', 'Profissional atualizado')
    },
    onError: () => toast('error', 'Erro ao atualizar profissional'),
  })
}
