import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import type { components } from '../api/schema'

export type CalendarAppointment = components['schemas']['CalendarAppointment']
export type CalendarAbsence = components['schemas']['CalendarAbsence']
export type CalendarDay = components['schemas']['CalendarDay']
export type CalendarResponse = components['schemas']['CalendarResponse']
export type FilledSlotsResponse = components['schemas']['FilledSlotsResponse']
export type ProfessionalStatsResponse = components['schemas']['ProfessionalStatsResponse']

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
