import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import type { PagedResponse } from './useAppointments'

export interface PatientSummary {
  clientId: string
  name: string
  email: string
  phone: string
  lastAppointmentDate: string | null
  totalAppointments: number
}

export function useMyPatients(page: number, pageSize = 10, search?: string) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (search) params.set('search', search)

  return useQuery({
    queryKey: ['professionals', 'my-patients', page, pageSize, search],
    queryFn: () => api.get<PagedResponse<PatientSummary>>(`/professionals/my-patients?${params}`),
  })
}
