import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface StatusCount {
  status: string
  count: number
}

export interface AppointmentsByStatusResponse {
  items: StatusCount[]
}

export interface TimelinePoint {
  period: string
  scheduled: number
  completed: number
  cancelled: number
  noShow: number
}

export interface AppointmentsTimelineResponse {
  points: TimelinePoint[]
}

export interface HourSlot {
  dayOfWeek: number
  hour: number
  count: number
}

export interface BusiestHoursResponse {
  slots: HourSlot[]
}

export interface GrowthPoint {
  month: string
  newClients: number
  totalClients: number
}

export interface PatientGrowthResponse {
  points: GrowthPoint[]
}

export interface RevenuePoint {
  month: string
  revenue: number
  appointmentCount: number
}

export interface RevenueTimelineResponse {
  points: RevenuePoint[]
}

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
