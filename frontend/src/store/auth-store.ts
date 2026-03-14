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
    return {
      tenantId: payload.tenant_id ?? null,
      role: payload.role ?? null,
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
    if (raw) return JSON.parse(raw)
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
