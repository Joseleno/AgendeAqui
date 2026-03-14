import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'

export interface Absence {
  id: string
  professionalId: string
  date: string
  startTime: string | null
  endTime: string | null
  reason: string | null
  isFullDay: boolean
}

export function useAbsences(professionalId?: string, from?: string, to?: string) {
  const params = new URLSearchParams()
  if (professionalId) params.set('professionalId', professionalId)
  if (from) params.set('from', from)
  if (to) params.set('to', to)

  return useQuery({
    queryKey: ['absences', professionalId, from, to],
    queryFn: () => api.get<Absence[]>(`/absences?${params}`),
    enabled: !!from && !!to,
  })
}

export function useCreateAbsence() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: { professionalId: string; date: string; startTime?: string; endTime?: string; reason?: string }) =>
      api.post<{ id: string }>('/absences', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['absences'] })
      toast('success', 'Ausência criada')
    },
    onError: () => toast('error', 'Erro ao criar ausência'),
  })
}

export function useDeleteAbsence() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/absences/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['absences'] })
      toast('success', 'Ausência removida')
    },
    onError: () => toast('error', 'Erro ao remover ausência'),
  })
}
