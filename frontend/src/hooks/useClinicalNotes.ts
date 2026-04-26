import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

export type ClinicalNote = components['schemas']['ClinicalNote']
export type PagedClinicalNotes = components['schemas']['PagedResponse_ClinicalNote']

export function useClinicalNotes(clientId: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['clinical-notes', clientId, page, pageSize],
    queryFn: () => api.get<PagedClinicalNotes>(`/clinical-notes?clientId=${clientId}&page=${page}&pageSize=${pageSize}`),
    enabled: !!clientId,
  })
}

export function useClinicalNote(id: string) {
  return useQuery({
    queryKey: ['clinical-notes', 'detail', id],
    queryFn: () => api.get<ClinicalNote>(`/clinical-notes/${id}`),
    enabled: !!id,
  })
}

export function useCreateClinicalNote() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: { clientId: string; appointmentId?: string; title: string; content: string; isPrivate: boolean }) =>
      api.post<{ id: string }>('/clinical-notes', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clinical-notes'] })
      toast('success', 'Nota adicionada')
    },
    onError: () => toast('error', 'Erro ao criar nota'),
  })
}

export function useUpdateClinicalNote() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...payload }: { id: string; title: string; content: string; isPrivate: boolean }) =>
      api.put(`/clinical-notes/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clinical-notes'] })
      toast('success', 'Nota atualizada')
    },
    onError: () => toast('error', 'Erro ao atualizar nota'),
  })
}

export function useDeleteClinicalNote() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.delete(`/clinical-notes/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clinical-notes'] })
      toast('success', 'Nota removida')
    },
    onError: () => toast('error', 'Erro ao remover nota'),
  })
}
