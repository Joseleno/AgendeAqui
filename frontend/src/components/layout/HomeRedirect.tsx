import { useEffect } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuthState } from '../../hooks/useAuth'
import { authStore } from '../../store/auth-store'

export function HomeRedirect() {
  const { isAuthenticated, isAdmin, isProfessional, isClient } = useAuthState()
  const hasValidRole = isAdmin || isProfessional || isClient

  useEffect(() => {
    if (isAuthenticated && !hasValidRole) {
      authStore.clear()
    }
  }, [isAuthenticated, hasValidRole])

  if (!isAuthenticated || !hasValidRole) return <Navigate to="/login" replace />
  if (isAdmin) return <Navigate to="/dashboard" replace />
  if (isProfessional) return <Navigate to="/meu-calendario" replace />

  return <Navigate to="/meus-agendamentos" replace />
}
