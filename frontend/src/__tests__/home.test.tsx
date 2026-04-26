import { describe, it, expect, beforeAll, afterAll, afterEach, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { setupServer } from 'msw/node'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { handlers } from './handlers'
import { HomePage } from '../pages/home/HomePage'
import { authStore } from '../store/auth-store'

vi.mock('../lib/signalr', () => ({
  getConnection: () => ({ on: vi.fn(), off: vi.fn(), start: vi.fn().mockResolvedValue(undefined), stop: vi.fn().mockResolvedValue(undefined), state: 'Disconnected' }),
  startConnection: vi.fn().mockResolvedValue(undefined),
  stopConnection: vi.fn().mockResolvedValue(undefined),
}))

const server = setupServer(...handlers)

beforeAll(() => server.listen({ onUnhandledRequest: 'bypass' }))
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

function renderHome() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

function fakeJwt(payload: Record<string, string>) {
  const header = btoa(JSON.stringify({ alg: 'HS256' }))
  const body = btoa(JSON.stringify(payload))
  return `${header}.${body}.sig`
}

describe('HomePage', () => {
  beforeEach(() => {
    const token = fakeJwt({ tenant_id: 'tenant-1', role: 'Admin' })
    authStore.setTokens(token, 'fake-refresh-token')
  })

  it('renders Dashboard heading', () => {
    renderHome()
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
  })

  it('shows metric card labels after loading', async () => {
    renderHome()

    await waitFor(() => {
      expect(screen.getByText('Agendamentos hoje')).toBeInTheDocument()
      expect(screen.getByText('Agendamentos semana')).toBeInTheDocument()
      expect(screen.getByText('Profissionais ativos')).toBeInTheDocument()
      expect(screen.getByText('Pacientes registrados')).toBeInTheDocument()
    })
  })

  it('loads and shows dashboard metrics from API', async () => {
    renderHome()

    await waitFor(() => {
      expect(screen.getAllByText('12').length).toBeGreaterThan(0)  // appointmentsToday
      expect(screen.getAllByText('47').length).toBeGreaterThan(0)  // appointmentsThisWeek
      expect(screen.getAllByText('238').length).toBeGreaterThan(0) // registeredClients
    })
  })

  it('shows upcoming appointments section', async () => {
    renderHome()

    await waitFor(() => {
      expect(screen.getByText('Próximos agendamentos')).toBeInTheDocument()
    })
  })

  it('shows today\'s appointments in upcoming list', async () => {
    renderHome()

    await waitFor(() => {
      expect(screen.getByText(/Maria Santos/)).toBeInTheDocument()
    })
  })

  it('shows advanced charts section heading', () => {
    renderHome()
    expect(screen.getByText('Análise dos últimos 30 dias')).toBeInTheDocument()
  })
})
