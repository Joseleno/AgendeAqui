import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api-client'
import { toast } from '../components/ui/Toast'

export interface Payment {
  id: string
  appointmentId: string
  clientName: string
  serviceName: string
  amount: number
  method: string
  status: string
  notes: string | null
  createdAt: string
}

export interface PagedPayments {
  items: Payment[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface MethodSummary {
  method: string
  total: number
  count: number
}

export interface PaymentSummary {
  totalReceived: number
  totalPending: number
  totalRefunded: number
  paymentCount: number
  byMethod: MethodSummary[]
}

export function usePayments(page = 1, pageSize = 20, appointmentId?: string, from?: string, to?: string) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (appointmentId) params.set('appointmentId', appointmentId)
  if (from) params.set('from', from)
  if (to) params.set('to', to)
  return useQuery({
    queryKey: ['payments', page, pageSize, appointmentId, from, to],
    queryFn: () => api.get<PagedPayments>(`/payments?${params}`),
  })
}

export function usePaymentSummary(from: string, to: string) {
  return useQuery({
    queryKey: ['payments', 'summary', from, to],
    queryFn: () => api.get<PaymentSummary>(`/payments/summary?from=${from}&to=${to}`),
    enabled: !!from && !!to,
  })
}

export function useCreatePayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: { appointmentId: string; amount: number; method: string; notes?: string }) =>
      api.post<{ id: string }>('/payments', payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payments'] })
      toast('success', 'Pagamento registrado')
    },
    onError: () => toast('error', 'Erro ao registrar pagamento'),
  })
}

export function useUpdatePaymentStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      api.put(`/payments/${id}/status`, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payments'] })
      toast('success', 'Status atualizado')
    },
    onError: () => toast('error', 'Erro ao atualizar status'),
  })
}
