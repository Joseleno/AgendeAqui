import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface ProfessionalAttendance {
  professionalId: string
  professionalName: string
  total: number
  completed: number
  cancelled: number
  noShow: number
}

export interface AttendanceReport {
  totalAppointments: number
  totalCompleted: number
  totalCancelled: number
  totalNoShow: number
  breakdown: ProfessionalAttendance[]
}

export interface ServiceRevenue {
  serviceId: string
  serviceName: string
  unitPrice: number
  appointmentCount: number
  totalRevenue: number
}

export interface RevenueReport {
  totalRevenue: number
  byService: ServiceRevenue[]
}

export function useAttendanceReport(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'attendance', from, to],
    queryFn: () => api.get<AttendanceReport>(`/reports/attendance?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function useRevenueReport(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'revenue', from, to],
    queryFn: () => api.get<RevenueReport>(`/reports/revenue?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}
