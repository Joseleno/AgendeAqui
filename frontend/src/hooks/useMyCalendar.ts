import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface CalendarAppointment {
  id: string
  clientName: string
  serviceName: string
  startTime: string
  endTime: string
  status: string
}

export interface CalendarAbsence {
  id: string
  startTime: string | null
  endTime: string | null
  reason: string | null
  isFullDay: boolean
}

export interface CalendarDay {
  date: string
  appointments: CalendarAppointment[]
  absences: CalendarAbsence[]
}

export interface CalendarResponse {
  days: CalendarDay[]
}

export interface FilledSlotsResponse {
  date: string
  totalSlots: number
  filledSlots: number
  availableSlots: number
  slots: { startTime: string; endTime: string; isFilled: boolean }[]
}

export interface ProfessionalStatsResponse {
  total: number
  completed: number
  cancelled: number
  noShow: number
  revenue: number
}

export function useMyCalendar(dateFrom: string, dateTo: string) {
  return useQuery({
    queryKey: ['appointments', 'calendar', dateFrom, dateTo],
    queryFn: () => api.get<CalendarResponse>(`/appointments/calendar?dateFrom=${dateFrom}&dateTo=${dateTo}`),
    enabled: !!dateFrom && !!dateTo,
  })
}

export function useMyFilledSlots(date: string) {
  return useQuery({
    queryKey: ['appointments', 'filled-slots', date],
    queryFn: () => api.get<FilledSlotsResponse>(`/appointments/filled-slots?date=${date}`),
    enabled: !!date,
  })
}

export function useMyStats(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'my-stats', from, to],
    queryFn: () => api.get<ProfessionalStatsResponse>(`/reports/my-stats?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}
