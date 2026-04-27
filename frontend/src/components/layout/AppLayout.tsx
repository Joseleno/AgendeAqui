import { Outlet, Navigate, useLocation } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { Sidebar } from './Sidebar'
import { BottomNav } from './BottomNav'
import { Header } from './Header'
import { useAuthState } from '../../hooks/useAuth'
import { useSignalR } from '../../hooks/useSignalR'
import { useTenantContext } from '../../context/TenantContext'
import { toast } from '../ui/Toast'
import { differenceInDays } from 'date-fns'

function TrialBanner() {
  const { isOnTrial, trialEndsAt } = useTenantContext()
  if (!isOnTrial || !trialEndsAt) return null

  const daysLeft = Math.max(0, differenceInDays(trialEndsAt, new Date()))

  return (
    <div className="bg-amber-50 border-b border-amber-200 px-4 py-2 text-xs text-amber-800 flex items-center justify-center gap-1">
      <span className="font-semibold">Período de teste:</span>
      {daysLeft > 0
        ? <span>{daysLeft} dia{daysLeft !== 1 ? 's' : ''} restante{daysLeft !== 1 ? 's' : ''}. Faça o upgrade para continuar usando após o trial.</span>
        : <span>Seu trial expirou. Entre em contato para continuar.</span>
      }
    </div>
  )
}

export function AppLayout() {
  const { isAuthenticated } = useAuthState()
  const location = useLocation()
  const queryClient = useQueryClient()

  useSignalR('InAppNotificationReceived', (notification: unknown) => {
    const n = notification as { title?: string; message?: string }
    queryClient.invalidateQueries({ queryKey: ['notifications'] })
    if (n?.title) {
      toast('info', n.title, n.message)
    }
  })

  if (!isAuthenticated) return <Navigate to="/login" replace />

  return (
    <div className="flex min-h-screen bg-gray-50/80">
      <Sidebar />
      <div className="flex-1 flex flex-col min-w-0">
        <TrialBanner />
        <Header />
        <main className="flex-1 p-4 md:p-6 pb-20 md:pb-6 overflow-auto">
          <div key={location.pathname} className="page-enter">
            <Outlet />
          </div>
        </main>
      </div>
      <BottomNav />
    </div>
  )
}
