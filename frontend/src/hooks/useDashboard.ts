import { useQuery, useMutation } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'
import type { components } from '../api/schema'

export type DashboardOverview = components['schemas']['DashboardOverview']
export type ProfessionalRanking = components['schemas']['ProfessionalRanking']
export type ScheduleConflict = components['schemas']['ScheduleConflict']
export type ClinicAppointment = components['schemas']['ClinicAppointment']
export type ProfessionalDay = components['schemas']['ProfessionalDay']
export type ClinicCalendar = components['schemas']['ClinicCalendar']

export function useDashboardOverview() {
  return useQuery({
    queryKey: ['dashboard', 'overview'],
    queryFn: () => api.get<DashboardOverview>('/reports/dashboard'),
  })
}

export function useProfessionalRanking(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'ranking', from, to],
    queryFn: () => api.get<ProfessionalRanking[]>(`/reports/professional-ranking?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function useScheduleConflicts(date: string) {
  return useQuery({
    queryKey: ['schedules', 'conflicts', date],
    queryFn: () => api.get<ScheduleConflict[]>(`/schedules/conflicts?date=${date}`),
    enabled: !!date,
  })
}

export function useClinicCalendar(date: string) {
  return useQuery({
    queryKey: ['appointments', 'clinic-calendar', date],
    queryFn: () => api.get<ClinicCalendar>(`/appointments/clinic-calendar?date=${date}`),
    enabled: !!date,
  })
}

export function useExportCsv() {
  return useMutation({
    mutationFn: async ({ from, to, professionalId }: { from: string; to: string; professionalId?: string }) => {
      const params = new URLSearchParams({ from, to })
      if (professionalId) params.set('professionalId', professionalId)
      const csv = await api.getText(`/reports/export/appointments?${params}`)
      const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = 'agendamentos.csv'
      link.click()
      setTimeout(() => URL.revokeObjectURL(url), 5000)
    },
    onSuccess: () => toast('success', 'CSV exportado'),
    onError: () => toast('error', 'Erro ao exportar CSV'),
  })
}
