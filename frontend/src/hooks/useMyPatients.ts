import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import type { components } from '../api/schema'

export type PatientSummary = components['schemas']['PatientSummary']
type PagedResponse<T> = { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }

export function useMyPatients(page: number, pageSize = 10, search?: string) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (search) params.set('search', search)

  return useQuery({
    queryKey: ['professionals', 'my-patients', page, pageSize, search],
    queryFn: () => api.get<PagedResponse<PatientSummary>>(`/professionals/my-patients?${params}`),
  })
}
