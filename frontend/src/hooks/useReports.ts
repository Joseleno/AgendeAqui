import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import type { components } from '../api/schema'

export type ProfessionalAttendance = components['schemas']['ProfessionalAttendance']
export type AttendanceReport = components['schemas']['AttendanceReport']
export type ServiceRevenue = components['schemas']['ServiceRevenue']
export type RevenueReport = components['schemas']['RevenueReport']

export function useAttendanceReport(from: string, to: string, professionalId?: string) {
  const params = new URLSearchParams({ from, to })
  if (professionalId) params.set('professionalId', professionalId)

  return useQuery({
    queryKey: ['reports', 'attendance', from, to, professionalId],
    queryFn: () => api.get<AttendanceReport>(`/reports/attendance?${params}`),
    enabled: !!from && !!to,
  })
}

export function useRevenueReport(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'revenue', from, to],
    queryFn: () => api.get<RevenueReport>(`/reports/revenue?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}
