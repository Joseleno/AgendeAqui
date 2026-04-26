import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'

export interface TeamMemberItem {
  professionalId: string
  professionalName: string
  joinedAt: string
}

export interface TeamDetail {
  id: string
  name: string
  description: string | null
  leaderName: string | null
  isActive: boolean
  members: TeamMemberItem[]
}

export interface TeamSummary {
  id: string
  name: string
  description: string | null
  leaderName: string | null
  isActive: boolean
  memberCount: number
}

export interface TeamAppointmentBreakdown {
  teamId: string
  teamName: string
  total: number
  completed: number
  cancelled: number
  noShow: number
  completionRate: number
  cancellationRate: number
}

export interface UnassignedBreakdown {
  professionalId: string
  professionalName: string
  total: number
  completed: number
  cancelled: number
  noShow: number
}

export interface TeamAppointmentsReport {
  totalAppointments: number
  totalCompleted: number
  totalCancelled: number
  totalNoShow: number
  teams: TeamAppointmentBreakdown[]
  unassigned: UnassignedBreakdown[]
}

export function useTeams() {
  return useQuery({
    queryKey: ['teams'],
    queryFn: () => api.get<TeamSummary[]>('/teams'),
  })
}

export function useTeamAppointmentsReport(from: string, to: string) {
  return useQuery({
    queryKey: ['reports', 'teams', from, to],
    queryFn: () => api.get<TeamAppointmentsReport>(`/reports/teams?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function useCreateTeam() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: { name: string; description?: string }) =>
      api.post<{ id: string }>('/teams', data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['teams'] }),
  })
}

export function useAddTeamMember() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ teamId, professionalId }: { teamId: string; professionalId: string }) =>
      api.post(`/teams/${teamId}/members`, { professionalId }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['teams'] }),
  })
}

export function useRemoveTeamMember() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ teamId, professionalId }: { teamId: string; professionalId: string }) =>
      api.delete(`/teams/${teamId}/members/${professionalId}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['teams'] }),
  })
}

export function useDisbandTeam() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (teamId: string) => api.delete(`/teams/${teamId}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['teams'] }),
  })
}
