import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api-client'
import { authStore } from '../store/auth-store'
import { useSyncExternalStore } from 'react'

interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresInMinutes: number
}

export function useAuthState() {
  const state = useSyncExternalStore(
    authStore.subscribe,
    authStore.getState,
  )
  return {
    isAuthenticated: !!state.accessToken,
    tenantId: state.tenantId,
  }
}

export function useLogin() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (credentials: { email: string; password: string }) =>
      api.post<LoginResponse>('/auth/login', credentials),
    onSuccess: (data) => {
      authStore.setTokens(data.accessToken, data.refreshToken)
      // Extract tenant_id from JWT payload
      try {
        const payload = JSON.parse(atob(data.accessToken.split('.')[1]))
        if (payload.tenant_id) authStore.setTenantId(payload.tenant_id)
      } catch { /* ignore malformed token */ }
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
