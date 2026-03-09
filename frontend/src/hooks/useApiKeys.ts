import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'

export interface ApiKey {
  id: string
  name: string
  keyPrefix: string
  isActive: boolean
  expiresAt: string | null
  createdAt: string
}

export interface CreateApiKeyResponse {
  id: string
  rawKey: string
  name: string
  expiresAt: string | null
}

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
