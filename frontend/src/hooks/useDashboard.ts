import { useQuery, useMutation } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'

export interface DashboardOverview {
  appointmentsToday: number
  appointmentsThisWeek: number
  activeProfessionals: number
  registeredClients: number
  cancelledToday: number
  noShowToday: number
}

export interface ProfessionalRanking {
  professionalId: string
  name: string
  specialty: string | null
  total: number
  completed: number
  cancelled: number
  noShow: number
  completionRate: number
  noShowRate: number
}

export interface ScheduleConflict {
  professionalId: string
  professionalName: string
  appointmentId1: string
  appointmentId2: string
  startTime1: string
  endTime1: string
  startTime2: string
  endTime2: string
  client1: string
  client2: string
}

export interface ClinicAppointment {
  id: string
  clientName: string
  serviceName: string
  startTime: string
  endTime: string
  status: string
}

export interface ProfessionalDay {
  professionalId: string
  professionalName: string
  appointments: ClinicAppointment[]
}

export interface ClinicCalendar {
  date: string
  professionals: ProfessionalDay[]
}

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
