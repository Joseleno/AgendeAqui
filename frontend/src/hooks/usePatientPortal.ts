import { useQuery, useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api-client'
import { authStore } from '../store/auth-store'
import { toast } from '../components/ui/Toast'
import type { PagedResponse } from './useAppointments'

interface RegisterPayload {
  name: string
  email: string
  phone: string
  password: string
}

interface RegisterResponse {
  accessToken: string
  refreshToken: string
  expiresInMinutes: number
}

export interface ProfessionalSummary {
  id: string
  name: string
  specialty: string | null
  services: string[]
}

export interface ProfessionalAvailability {
  professionalId: string
  professionalName: string
  specialty: string | null
  availableSlots: number
}

export interface MyAppointment {
  id: string
  professionalName: string
  serviceName: string
  date: string
  startTime: string
  endTime: string
  status: string
  notes: string | null
}

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
