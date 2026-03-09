import { Outlet, Navigate } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { BottomNav } from './BottomNav'
import { Header } from './Header'
import { useAuthState } from '../../hooks/useAuth'

export function AppLayout() {
  const { isAuthenticated } = useAuthState()

  if (!isAuthenticated) return <Navigate to="/login" replace />

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header />
        <main className="flex-1 p-4 pb-20 md:pb-4 overflow-auto">
          <Outlet />
        </main>
      </div>
      <BottomNav />
    </div>
  )
}
