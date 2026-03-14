import { Navigate, Outlet } from 'react-router-dom'
import { useAuthState } from '../../hooks/useAuth'

export function RoleGuard({ roles }: { roles: string[] }) {
  const { role } = useAuthState()

  if (!role || !roles.includes(role)) return <Navigate to="/" replace />

  return <Outlet />
}
