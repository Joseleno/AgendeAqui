import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

export type ApiKey = components['schemas']['ApiKey']
export type CreateApiKeyResponse = components['schemas']['CreateApiKeyResponse']

export function useApiKeys() {
  return useQuery({
    queryKey: ['api-keys'],
    queryFn: () => api.get<ApiKey[]>('/api-keys'),
  })
}

export function useCreateApiKey() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: { name: string; expiresAt?: string }) =>
      api.post<CreateApiKeyResponse>('/api-keys', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['api-keys'] })
    },
    onError: () => toast('error', 'Erro ao criar API key'),
  })
}

export function useRevokeApiKey() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => api.delete(`/api-keys/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['api-keys'] })
      toast('success', 'API key revogada')
    },
    onError: () => toast('error', 'Erro ao revogar API key'),
  })
}
