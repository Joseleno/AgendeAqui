import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

export type Client = components['schemas']['Client']
type PagedResponse<T> = { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }

export function useClients(page: number, pageSize = 10) {
  return useQuery({
    queryKey: ['clients', page, pageSize],
    queryFn: () => api.get<PagedResponse<Client>>(`/clients?page=${page}&pageSize=${pageSize}`),
  })
}

export function useCreateClient() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: { name: string; email: string; phone: string }) =>
      api.post<{ id: string }>('/clients', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] })
      toast('success', 'Cliente criado')
    },
    onError: () => toast('error', 'Erro ao criar cliente'),
  })
}

export function useUpdateClient() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string; name: string; email: string; phone: string }) =>
      api.put(`/clients/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] })
      toast('success', 'Cliente atualizado')
    },
    onError: () => toast('error', 'Erro ao atualizar cliente'),
  })
}
