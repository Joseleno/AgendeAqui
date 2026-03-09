import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { useSignalR } from './useSignalR'
import { toast } from '../components/ui/Toast'

export interface Appointment {
  id: string
  professionalId: string
  professionalName: string
  serviceId: string
  serviceName: string
  clientId: string
  clientName: string
  date: string
  startTime: string
  endTime: string
  status: string
  notes: string | null
  createdAt: string
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

interface CreateAppointmentPayload {
  professionalId: string
  serviceId: string
  clientId: string
  date: string
  startTime: string
  notes?: string
}

interface AvailableSlot {
  start: string
  end: string
}

interface AvailabilityResponse {
  date: string
  professionalId: string
  slots: AvailableSlot[]
}

export function useAppointments(dateFrom: string, dateTo: string) {
  return useQuery({
    queryKey: ['appointments', dateFrom, dateTo],
    queryFn: () =>
      api.get<PagedResponse<Appointment>>(
        `/appointments?dateFrom=${dateFrom}&dateTo=${dateTo}&pageSize=200`,
      ),
  })
}

export function useAvailability(professionalId: string, date: string, serviceId: string) {
  return useQuery({
    queryKey: ['availability', professionalId, date, serviceId],
    queryFn: () =>
      api.get<AvailabilityResponse>(
        `/availability?professionalId=${professionalId}&date=${date}&serviceId=${serviceId}`,
      ),
    enabled: !!professionalId && !!date && !!serviceId,
  })
}

export function useCreateAppointment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateAppointmentPayload) =>
      api.post<{ id: string }>('/appointments', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
      toast('success', 'Agendamento criado')
    },
    onError: () => {
      toast('error', 'Erro ao criar agendamento')
    },
  })
}

export function useCancelAppointment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      api.post(`/appointments/${id}/cancel`, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
      toast('success', 'Agendamento cancelado')
    },
    onError: () => {
      toast('error', 'Erro ao cancelar agendamento')
    },
  })
}

export function useRescheduleAppointment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, newDate, newStartTime }: { id: string; newDate: string; newStartTime: string }) =>
      api.post(`/appointments/${id}/reschedule`, { newDate, newStartTime }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
      toast('success', 'Agendamento reagendado')
    },
    onError: () => {
      toast('error', 'Erro ao reagendar')
    },
  })
}

export function useAppointmentRealtime() {
  const queryClient = useQueryClient()

  useSignalR('AppointmentCreated', () => {
    queryClient.invalidateQueries({ queryKey: ['appointments'] })
  })

  useSignalR('AppointmentCancelled', () => {
    queryClient.invalidateQueries({ queryKey: ['appointments'] })
  })

  useSignalR('AppointmentRescheduled', () => {
    queryClient.invalidateQueries({ queryKey: ['appointments'] })
  })
}
