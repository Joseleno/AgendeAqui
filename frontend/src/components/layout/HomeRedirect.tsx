import { Navigate } from 'react-router-dom'
import { useAuthState } from '../../hooks/useAuth'

export function HomeRedirect() {
  const { isAdmin, isProfessional, isClient } = useAuthState()

  if (isAdmin) return <Navigate to="/dashboard" replace />
  if (isProfessional) return <Navigate to="/meu-calendario" replace />
  if (isClient) return <Navigate to="/meus-agendamentos" replace />

  return <Navigate to="/login" replace />
}
