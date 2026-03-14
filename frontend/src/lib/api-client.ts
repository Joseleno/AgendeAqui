import { authStore } from '../store/auth-store'

const BASE_URL = '/api/v1'

export class ApiError extends Error {
  status: number
  title: string
  detail: string

  constructor(status: number, title: string, detail: string) {
    super(detail)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.detail = detail
  }
}

async function request<T>(
  path: string,
  options: RequestInit = {},
  responseType: 'json' | 'text' = 'json',
): Promise<T> {
  const { accessToken, tenantId } = authStore.getState()

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...((options.headers as Record<string, string>) ?? {}),
  }

  if (accessToken) headers['Authorization'] = `Bearer ${accessToken}`
  if (tenantId) headers['X-Tenant-Id'] = tenantId

  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers,
  })

  if (response.status === 401) {
    // Only clear auth and redirect if we had a token (session expired).
    // Avoid redirect loops when login itself returns 401.
    if (accessToken) {
      authStore.clear()
      window.location.href = '/login'
    }
    throw new ApiError(401, 'Unauthorized', 'Session expired')
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => ({}))
    throw new ApiError(
      response.status,
      problem.title ?? 'Error',
      problem.detail ?? response.statusText,
    )
  }

  if (response.status === 204) return undefined as T
  if (responseType === 'text') return response.text() as T

  return response.json()
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  getText: (path: string) => request<string>(path, {}, 'text'),

  post: <T>(path: string, body?: unknown) =>
    request<T>(path, {
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    }),

  put: <T>(path: string, body?: unknown) =>
    request<T>(path, {
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    }),

  delete: <T>(path: string) =>
    request<T>(path, { method: 'DELETE' }),
}
