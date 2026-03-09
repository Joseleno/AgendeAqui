interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  tenantId: string | null
}

const STORAGE_KEY = 'agendeaqui_auth'

function load(): AuthState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) return JSON.parse(raw)
  } catch { /* ignore */ }
  return { accessToken: null, refreshToken: null, tenantId: null }
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
    state = { ...state, accessToken, refreshToken }
    persist()
    notify()
  },

  setTenantId(tenantId: string) {
    state = { ...state, tenantId }
    persist()
    notify()
  },

  clear() {
    state = { accessToken: null, refreshToken: null, tenantId: null }
    localStorage.removeItem(STORAGE_KEY)
    notify()
  },

  subscribe(fn: () => void) {
    listeners.add(fn)
    return () => listeners.delete(fn)
  },
}
