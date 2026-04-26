import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

export type Webhook = components['schemas']['Webhook']

export function useWebhooks() {
  return useQuery({
    queryKey: ['webhooks'],
    queryFn: () => api.get<Webhook[]>('/webhooks'),
  })
}

export function useCreateWebhook() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: { url: string; secret: string; events: string[] }) =>
      api.post<{ id: string }>('/webhooks', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['webhooks'] })
      toast('success', 'Webhook criado')
    },
    onError: () => toast('error', 'Erro ao criar webhook'),
  })
}

export function useUpdateWebhook() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string; url: string; events: string[]; isActive: boolean }) =>
      api.put(`/webhooks/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['webhooks'] })
      toast('success', 'Webhook atualizado')
    },
    onError: () => toast('error', 'Erro ao atualizar webhook'),
  })
}

export function useDeleteWebhook() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => api.delete(`/webhooks/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['webhooks'] })
      toast('success', 'Webhook removido')
    },
    onError: () => toast('error', 'Erro ao remover webhook'),
  })
}
