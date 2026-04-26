import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { authStore } from '../store/auth-store'

export interface TenantLabels {
  professional: string
  professionals: string
  client: string
  clients: string
  appointment: string
  appointments: string
  service: string
  services: string
  team: string
  teams: string
}

export interface TenantFeatures {
  hasClinicalNotes: boolean
  hasTeleconsultation: boolean
  hasTeams: boolean
  hasPayments: boolean
}

interface TenantContextResponse {
  tenantName: string
  labels: TenantLabels
  features: TenantFeatures
}

export const DEFAULT_LABELS: TenantLabels = {
  professional: 'Profissional',
  professionals: 'Profissionais',
  client: 'Cliente',
  clients: 'Clientes',
  appointment: 'Agendamento',
  appointments: 'Agendamentos',
  service: 'Serviço',
  services: 'Serviços',
  team: 'Equipe',
  teams: 'Equipes',
}

export const DEFAULT_FEATURES: TenantFeatures = {
  hasClinicalNotes: true,
  hasTeleconsultation: true,
  hasTeams: true,
  hasPayments: true,
}

interface TenantContextValue {
  labels: TenantLabels
  features: TenantFeatures
  tenantName: string | null
}

const TenantContext = createContext<TenantContextValue>({
  labels: DEFAULT_LABELS,
  features: DEFAULT_FEATURES,
  tenantName: null,
})

export function TenantProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(authStore.isAuthenticated())

  useEffect(() => {
    const unsub = authStore.subscribe(() => {
      setIsAuthenticated(authStore.isAuthenticated())
    })
    return () => { unsub() }
  }, [])

  const { data } = useQuery({
    queryKey: ['tenant', 'context'],
    queryFn: () => api.get<TenantContextResponse>('/tenant/context'),
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000,
  })

  return (
    <TenantContext.Provider
      value={{
        labels: data?.labels ?? DEFAULT_LABELS,
        features: data?.features ?? DEFAULT_FEATURES,
        tenantName: data?.tenantName ?? null,
      }}
    >
      {children}
    </TenantContext.Provider>
  )
}

export function useTenantContext() {
  return useContext(TenantContext)
}
