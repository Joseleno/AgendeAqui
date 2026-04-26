import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import type { components } from '../api/schema'

export type StatusCount = components['schemas']['StatusCount']
export type AppointmentsByStatusResponse = components['schemas']['AppointmentsByStatusResponse']
export type TimelinePoint = components['schemas']['TimelinePoint']
export type AppointmentsTimelineResponse = components['schemas']['AppointmentsTimelineResponse']
export type HourSlot = components['schemas']['HourSlot']
export type BusiestHoursResponse = components['schemas']['BusiestHoursResponse']
export type GrowthPoint = components['schemas']['GrowthPoint']
export type PatientGrowthResponse = components['schemas']['PatientGrowthResponse']
export type RevenuePoint = components['schemas']['RevenuePoint']
export type RevenueTimelineResponse = components['schemas']['RevenueTimelineResponse']

export function useAppointmentsByStatus(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'appointments-by-status', from, to],
    queryFn: () => api.get<AppointmentsByStatusResponse>(`/reports/appointments-by-status?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function useAppointmentsTimeline(from: string, to: string, groupBy = 'day') {
  return useQuery({
    queryKey: ['reports', 'appointments-timeline', from, to, groupBy],
    queryFn: () => api.get<AppointmentsTimelineResponse>(`/reports/appointments-timeline?from=${from}&to=${to}&groupBy=${groupBy}`),
    enabled: !!from && !!to,
  })
}

export function useBusiestHours(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'busiest-hours', from, to],
    queryFn: () => api.get<BusiestHoursResponse>(`/reports/busiest-hours?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function usePatientGrowth(months = 12) {
  return useQuery({
    queryKey: ['reports', 'patient-growth', months],
    queryFn: () => api.get<PatientGrowthResponse>(`/reports/patient-growth?months=${months}`),
  })
}

export function useRevenueTimeline(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'revenue-timeline', from, to],
    queryFn: () => api.get<RevenueTimelineResponse>(`/reports/revenue-timeline?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}
