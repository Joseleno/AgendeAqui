import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface TenantAppointmentSummary {
  tenantId: string
  tenantName: string
  tenantSlug: string
  total: number
  completed: number
  cancelled: number
  noShow: number
  completionRate: number
  activeProfessionals: number
}

export interface PlatformDashboard {
  activeTenants: number
  totalTenants: number
  totalAppointmentsInPeriod: number
  tenants: TenantAppointmentSummary[]
}

export function usePlatformDashboard(from: string, to: string) {
  return useQuery({
    queryKey: ['platform', 'dashboard', from, to],
    queryFn: () => api.get<PlatformDashboard>(`/platform/dashboard?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}
