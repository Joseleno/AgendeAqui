interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  tenantId: string | null
  role: string | null
  professionalId: string | null
  clientId: string | null
}

const STORAGE_KEY = 'agendeaqui_auth'

function decodeToken(token: string): Partial<AuthState> {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(atob(base64))
    const role = payload.role
      ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      ?? null
    return {
      tenantId: payload.tenant_id ?? null,
      role,
      professionalId: payload.professional_id ?? null,
      clientId: payload.client_id ?? null,
    }
  } catch {
    return { tenantId: null, role: null, professionalId: null, clientId: null }
  }
}

function load(): AuthState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) {
      const stored = JSON.parse(raw) as AuthState
      // Re-decode token to pick up any claim extraction fixes
      if (stored.accessToken) {
        const decoded = decodeToken(stored.accessToken)
        return { ...stored, ...decoded }
      }
      return stored
    }
  } catch { /* ignore */ }
  return {
    accessToken: null,
    refreshToken: null,
    tenantId: null,
    role: null,
    professionalId: null,
    clientId: null,
  }
}

let state = load()
const listeners = new Set<() => void>()

function notify() {
  listeners.forEach((fn) => fn())
}

function persist() {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
}

export const authStore = {
  getState: () => state,

  isAuthenticated: () => !!state.accessToken,

  setTenantId(tenantId: string) {
    if (state.tenantId === tenantId) return
    state = { ...state, tenantId }
    persist()
    notify()
  },

  setTokens(accessToken: string, refreshToken: string) {
    const decoded = decodeToken(accessToken)
    state = { ...state, accessToken, refreshToken, ...decoded }
    persist()
    notify()
  },

  clear() {
    state = {
      accessToken: null,
      refreshToken: null,
      tenantId: null,
      role: null,
      professionalId: null,
      clientId: null,
    }
    localStorage.removeItem(STORAGE_KEY)
    notify()
  },

  subscribe(fn: () => void) {
    listeners.add(fn)
    return () => listeners.delete(fn)
  },
}
