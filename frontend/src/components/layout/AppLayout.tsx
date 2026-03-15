import { Outlet, Navigate, useLocation } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { Sidebar } from './Sidebar'
import { BottomNav } from './BottomNav'
import { Header } from './Header'
import { useAuthState } from '../../hooks/useAuth'
import { useSignalR } from '../../hooks/useSignalR'
import { toast } from '../ui/Toast'

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
