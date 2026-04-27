import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
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

export interface TenantUsage {
  tenantId: string
  name: string
  slug: string
  plan: string
  status: string
  isOnTrial: boolean
  trialEndsAt?: string | null
  customDomain?: string | null
  professionalsCount: number
  maxProfessionals: number
  clientsCount: number
  maxClients: number
  appointmentsThisMonth: number
  maxAppointmentsPerMonth: number
}

export function usePlatformDashboard(from: string, to: string) {
  return useQuery({
    queryKey: ['platform', 'dashboard', from, to],
    queryFn: () => api.get<PlatformDashboard>(`/platform/dashboard?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function useTenantUsage(tenantId: string | undefined) {
  return useQuery({
    queryKey: ['platform', 'tenant-usage', tenantId],
    queryFn: () => api.get<TenantUsage>(`/platform/tenants/${tenantId}/usage`),
    enabled: !!tenantId,
  })
}

export function useSuspendTenant() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (tenantId: string) => api.post(`/platform/tenants/${tenantId}/suspend`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platform'] })
    },
  })
}

export function useActivateTenant() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (tenantId: string) => api.post(`/platform/tenants/${tenantId}/activate`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platform'] })
    },
  })
}

export function useChangeTenantPlan() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ tenantId, plan }: { tenantId: string; plan: string }) =>
      api.put(`/platform/tenants/${tenantId}/plan`, { plan }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platform'] })
    },
  })
}

export function useSetCustomDomain() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ tenantId, domain }: { tenantId: string; domain: string | null }) =>
      api.put(`/platform/tenants/${tenantId}/custom-domain`, { domain }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['platform'] })
    },
  })
}
