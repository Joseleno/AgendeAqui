import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api-client'
import { authStore } from '../store/auth-store'
import { useSyncExternalStore } from 'react'
import type { components } from '../api/schema'

type LoginResponse = components['schemas']['LoginResponse']

export function useAuthState() {
  const state = useSyncExternalStore(
    authStore.subscribe,
    authStore.getState,
  )
  return {
    isAuthenticated: !!state.accessToken,
    tenantId: state.tenantId,
    role: state.role,
    professionalId: state.professionalId,
    clientId: state.clientId,
    isAdmin: state.role === 'Admin',
    isProfessional: state.role === 'Professional',
    isClient: state.role === 'Client',
  }
}

export function useLogin() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (credentials: { email: string; password: string }) =>
      api.post<LoginResponse>('/auth/login', credentials),
    onSuccess: (data) => {
      authStore.setTokens(data.accessToken, data.refreshToken)
      navigate('/')
    },
  })
}

export function useLogout() {
  const navigate = useNavigate()

  return () => {
    authStore.clear()
    navigate('/login')
  }
}
