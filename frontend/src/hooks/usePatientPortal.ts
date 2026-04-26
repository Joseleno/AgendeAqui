import { useQuery, useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api-client'
import { authStore } from '../store/auth-store'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

type RegisterPayload = components['schemas']['RegisterRequest']
type RegisterResponse = components['schemas']['LoginResponse']
type PagedResponse<T> = { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }

export type ProfessionalSummary = components['schemas']['ProfessionalSummary']
export type ProfessionalAvailability = components['schemas']['ProfessionalAvailability']
export type MyAppointment = components['schemas']['MyAppointment']

export function useRegisterPatient() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (payload: RegisterPayload) =>
      api.post<RegisterResponse>('/auth/register', payload),
    onSuccess: (data) => {
      authStore.setTokens(data.accessToken, data.refreshToken)
      toast('success', 'Cadastro realizado com sucesso!')
      navigate('/meus-agendamentos')
    },
    onError: (err) => {
      const detail = err instanceof Error && 'detail' in err ? (err as { detail: string }).detail : ''
      if (detail.includes('already')) {
        toast('error', 'Este email já está cadastrado')
      } else {
        toast('error', detail || 'Erro ao realizar cadastro')
      }
    },
  })
}

export function useSearchProfessionals(name?: string, specialty?: string, serviceId?: string, page = 1) {
  const params = new URLSearchParams({ page: String(page), pageSize: '10' })
  if (name) params.set('name', name)
  if (specialty) params.set('specialty', specialty)
  if (serviceId) params.set('serviceId', serviceId)

  return useQuery({
    queryKey: ['professionals', 'search', name, specialty, serviceId, page],
    queryFn: () => api.get<PagedResponse<ProfessionalSummary>>(`/professionals/search?${params}`),
  })
}

export function useAvailableProfessionals(serviceId: string, date: string) {
  return useQuery({
    queryKey: ['availability', 'professionals', serviceId, date],
    queryFn: () => api.get<ProfessionalAvailability[]>(`/availability/professionals?serviceId=${serviceId}&date=${date}`),
    enabled: !!serviceId && !!date,
  })
}

export function useMyAppointments(page: number, pageSize = 10, status?: string) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (status) params.set('status', status)

  return useQuery({
    queryKey: ['appointments', 'mine', page, pageSize, status],
    queryFn: () => api.get<PagedResponse<MyAppointment>>(`/appointments/mine?${params}`),
  })
}
