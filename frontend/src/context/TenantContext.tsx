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

export interface TenantTheme {
  primaryColor: string
  logoUrl?: string | null
  faviconUrl?: string | null
}

interface TenantContextResponse {
  tenantName: string
  plan: string
  isOnTrial: boolean
  trialEndsAt?: string | null
  labels: TenantLabels
  features: TenantFeatures
  theme: TenantTheme
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

export const DEFAULT_THEME: TenantTheme = {
  primaryColor: '#6366f1',
  logoUrl: null,
  faviconUrl: null,
}

interface TenantContextValue {
  labels: TenantLabels
  features: TenantFeatures
  theme: TenantTheme
  tenantName: string | null
  plan: string | null
  isOnTrial: boolean
  trialEndsAt: Date | null
}

const TenantContext = createContext<TenantContextValue>({
  labels: DEFAULT_LABELS,
  features: DEFAULT_FEATURES,
  theme: DEFAULT_THEME,
  tenantName: null,
  plan: null,
  isOnTrial: false,
  trialEndsAt: null,
})

function applyTheme(theme: TenantTheme) {
  const root = document.documentElement
  // Convert hex #rrggbb to CSS custom properties for Tailwind brand-* shades
  root.style.setProperty('--color-brand', theme.primaryColor)

  if (theme.faviconUrl) {
    const link = document.querySelector<HTMLLinkElement>('link[rel~="icon"]')
    if (link) link.href = theme.faviconUrl
  }
}

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

  useEffect(() => {
    applyTheme(data?.theme ?? DEFAULT_THEME)
  }, [data?.theme])

  return (
    <TenantContext.Provider
      value={{
        labels: data?.labels ?? DEFAULT_LABELS,
        features: data?.features ?? DEFAULT_FEATURES,
        theme: data?.theme ?? DEFAULT_THEME,
        tenantName: data?.tenantName ?? null,
        plan: data?.plan ?? null,
        isOnTrial: data?.isOnTrial ?? false,
        trialEndsAt: data?.trialEndsAt ? new Date(data.trialEndsAt) : null,
      }}
    >
      {children}
    </TenantContext.Provider>
  )
}

export function useTenantContext() {
  return useContext(TenantContext)
}
